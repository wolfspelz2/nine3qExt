﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using JsonPath;
using n3q.Tools;
using n3q.Aspects;
using n3q.GrainInterfaces;
using n3q.Items;

namespace n3q.WebIt.Controllers
{
    [ApiController]
    public class RpcController : ControllerBase
    {
        public ICallbackLogger Log { get; set; }
        public WebItConfigDefinition Config { get; set; }
        public IItemClusterClient ItemClient { get; set; }

        public RpcController(ILogger<RpcController> logger, WebItConfigDefinition config, IClusterClient clusterClient)
        {
            Log = new FrameworkCallbackLogger(logger);
            Config = config;
            ItemClient = new OrleansItemClusterClient(clusterClient);
        }

        //public RpcController(WebItConfigDefinition config, SiloSimulator siloSimulator)
        //{
        //    Log = new NullCallbackLogger();
        //    Config = config;
        //    ItemClient = new SiloSimulatorClusterClient(siloSimulator);
        //}

        ItemStub MakeItemStub(string itemId)
        {
            var itemStub = new ItemStub(ItemClient.GetItemClient(itemId));
            return itemStub;
        }

        [Route("[controller]")]
        [HttpPost]
        public async Task<string> Post()
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            return await Get(body);
        }

        [Route("[controller]/{action}")]
        [HttpGet]
        public async Task<string> Get(string body)
        {
            var response = new JsonPath.Dictionary();

            try {
                var request = new JsonPath.Node(body).AsDictionary;

                var method = request["method"].AsString.Capitalize();

                Log.Info(method);

                switch (method) {
                    case nameof(Echo): response = Echo(request); break;
                    case nameof(GetPayloadHash): response = GetPayloadHash(request); break;
                    case nameof(GetItemProperties): response = await GetItemProperties(request); break;
                    case nameof(ChangeItemProperties): response = await ChangeItemProperties(request); break;
                    default: throw new Exception($"Unknown method={method}");
                }

                response["status"] = "ok";

            } catch (Exception ex) {
                response["status"] = "error";
                response["message"] = ex.Message;
            }

            return response.ToNode().ToJson();
        }

        [Route("[controller]/{action}")]
        public JsonPath.Dictionary Echo(JsonPath.Dictionary request)
        {
            return request
                .Select(pair => new KeyValuePair<string, Node>(pair.Key, new JsonPath.Node(Node.Type.Auto, pair.Value)))
                .ToDictionary()
                ;
        }

        [Route("[controller]/{action}")]
        public JsonPath.Dictionary GetPayloadHash(JsonPath.Dictionary request)
        {
            var payloadNode = request["payload"];
            if (payloadNode.AsDictionary.Count == 0) { throw new Exception("No payload"); }

            var hash = Aspects.Developer.ComputePayloadHash(Config.PayloadHashSecret, payloadNode);

            return new JsonPath.Dictionary().Add("result", hash);
        }

        [Route("[controller]/{action}")]
        public async Task<Dictionary> GetItemProperties(JsonPath.Dictionary request)
        {
            var partnerToken = request["partner"].AsString;
            var partnerId = await GetPartnerIdAndValidateThePartnerToken(partnerToken);

            var contextToken = request["context"].AsString;
            var context = GetContextAndValidateTheContextToken(contextToken);

            var pids = new PidSet(request["pids"].AsList
                .Select(pidNode => pidNode.AsString.ToEnum(Pid.Unknown))
                .Where(pid => Property.GetDefinition(pid).Access == Property.Access.Public)
            );
            pids.Add(Pid.Developer);

            var props = await MakeItemStub(context.itemId).Get(pids);

            if (props.GetString(Pid.Developer) != partnerId) { throw new Exception("Partner invalid"); }

            var propsNode = new Node(Node.Type.Dictionary);
            foreach (var pair in props) {
                propsNode.AsDictionary.Add(pair.Key.ToString(), pair.Value.ToString());
            }

            var response = new JsonPath.Dictionary {
                { "result", propsNode }
            };
            return response;
        }

        [Route("[controller]/{action}")]
        public async Task<Dictionary> ChangeItemProperties(JsonPath.Dictionary request)
        {
            var partnerToken = request["partner"].AsString;
            var partnerId = await GetPartnerIdAndValidateThePartnerToken(partnerToken);

            var contextToken = request["context"].AsString;
            var context = GetContextAndValidateTheContextToken(contextToken);

            var propPartnerId = await MakeItemStub(context.itemId).GetItemId(Pid.Developer);
            if (propPartnerId != partnerId) { throw new Exception("Partner invalid"); }

            var setNode = request["set"];
            var setPids = new PidSet(setNode.AsDictionary.Keys
                .Select(pidName => pidName.ToEnum(Pid.Unknown))
                .Where(pid => Property.GetDefinition(pid).Access == Property.Access.Public)
            );
            var modifyProps = new PropertySet();
            foreach (var pid in setPids) {
                var value = setNode[pid.ToString()].AsString;
                modifyProps[pid] = value;
            }

            var deletePids = new PidSet(request["delete"].AsList
                .Select(pidName => pidName.AsString.ToEnum(Pid.Unknown))
                .Where(pid => Property.GetDefinition(pid).Access == Property.Access.Public)
            );

            var item = MakeItemStub(context.itemId);
            item.WithTransaction(async self => {
                await self.Modify(modifyProps, deletePids);
            }).Wait();

            var response = new JsonPath.Dictionary {
            };
            return response;
        }

        [Route("[controller]/{action}")]
        public async Task<string> GetPartnerIdAndValidateThePartnerToken(string tokenBase64Encoded)
        {
            if (!Has.Value(tokenBase64Encoded)) { throw new Exception("No partner token"); }

            var tokenString = Tools.Base64.Decode(tokenBase64Encoded);
            var tokenNode = new JsonPath.Node(tokenString);
            var payloadNode = tokenNode["payload"];

            var partnerId = payloadNode["partner"].AsString;
            if (!Has.Value(partnerId)) { throw new Exception("No id in partner token"); }

            var props = await MakeItemStub(partnerId).Get(new PidSet { Pid.DeveloperAspect, Pid.PartnerToken });
            if (!props[Pid.DeveloperAspect]) { throw new Exception("Invalid partner token"); }
            if (props[Pid.PartnerToken] != tokenBase64Encoded) { throw new Exception("Invalid partner token"); }

            return partnerId;
        }

        public class RequestContext
        {
            public string userId;
            public string itemId;
            public DateTime expires;
        }

        [Route("[controller]/{action}")]
        public RequestContext GetContextAndValidateTheContextToken(string tokenBase64Encoded)
        {
            if (!Has.Value(tokenBase64Encoded)) { throw new Exception("No context token"); }

            var rc = new RequestContext();

            var tokenString = Tools.Base64.Decode(tokenBase64Encoded);
            var tokenNode = new JsonPath.Node(tokenString);

            var hash = tokenNode["hash"];
            var payloadNode = tokenNode["payload"];
            if (!Has.Value(payloadNode.AsString)) { throw new Exception("No payload"); }
            var computedHash = Aspects.Developer.ComputePayloadHash(Config.PayloadHashSecret, payloadNode);
            if (hash != computedHash)  { throw new Exception("Hash mismatch"); }

            rc.userId = payloadNode["user"];
            rc.itemId = payloadNode["item"];
            rc.expires = payloadNode["expires"];

            if (!Has.Value(rc.userId)) { throw new Exception("No user in context"); }
            if (!Has.Value(rc.itemId)) { throw new Exception("No item in context"); }

            return rc;
        }

    }
}