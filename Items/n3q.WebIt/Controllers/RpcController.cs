using System;
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
        public IClusterClient ClusterClient { get; set; }

        public RpcController(ILogger<RpcController> logger, WebItConfigDefinition config, IClusterClient clusterClient)
        {
            Log = new FrameworkCallbackLogger(logger);
            Config = config;
            ClusterClient = clusterClient;
        }

        ItemStub MakeItemStub(string itemId)
        {
            var itemClient = new OrleansClusterClient(ClusterClient, itemId);
            var itemStub = new ItemStub(itemClient);
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

                var method = request["method"].AsString;
                method = ("" + method[0]).ToUpper() + method.Substring(1);
                switch (method) {
                    case nameof(Echo): response = Echo(request); break;
                    case nameof(ComputePayloadHash): response = GetPayloadHash(request); break;
                    case nameof(GetItemProperties): response = await GetItemProperties(request); break;
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
            if (!request.ContainsKey("payload")) { throw new Exception("No payload"); }

            var payloadNode = request["payload"];
            if (!Has.Value(payloadNode.AsString)) { throw new Exception("No payload"); }
            var payload = payloadNode.ToJson();
            if (payload == "{}") { throw new Exception("No payload"); }

            var hash = ComputePayloadHash(payload);

            return new JsonPath.Dictionary().Add("result", hash);
        }

        private string ComputePayloadHash(string payload)
        {
            var data = Config.PayloadHashSecret + payload;
            var hash = Tools.Crypto.SHA256Base64(data);
            return hash;
        }

        [Route("[controller]/{action}")]
        public async Task<Dictionary> GetItemProperties(JsonPath.Dictionary request)
        {
            var itemId = request["item"].String;
            if (!Has.Value(itemId)) { throw new Exception("No item"); }

            var partnerToken = request["partner"].String;
            if (!Has.Value(partnerToken)) { throw new Exception("No partner token"); }
            await ValidatePartnerToken(partnerToken);

            var contextToken = request["context"].String;
            if (!Has.Value(contextToken)) { throw new Exception("No context token"); }
            var context = GetContext(contextToken);

            var pids = new PidSet();
            var pidsNode = request["pids"];
            foreach (var pidNode in pidsNode.AsList) {
                var pidName = (string)pidNode;
                var pid = pidName.ToEnum(Pid.Unknown);
                if (pid != Pid.Unknown) {
                    if (Property.GetDefinition(pid).Access == Property.Access.Public) {
                        pids.Add(pid);
                    }
                }
            }

            var props = await ClusterClient.GetGrain<IItem>(itemId).GetPropertiesX(pids);

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
        public async Task TestValidatePartnerToken(string tokenBase64Encoded) { await ValidatePartnerToken(tokenBase64Encoded); }
        private async Task ValidatePartnerToken(string tokenBase64Encoded)
        {
            var tokenString = Tools.Base64.Decode(tokenBase64Encoded);
            var tokenNode = new JsonPath.Node(tokenString);
            var payloadNode = tokenNode["payload"];

            var partnerId = payloadNode["partner"];
            if (!Has.Value(partnerId)) { throw new Exception("No partnerId in partner token"); }

            var props = await ClusterClient.GetGrain<IItem>(partnerId).GetPropertiesX(new PidSet { Pid.PartnerAspect, Pid.PartnerToken });
            if (!props[Pid.PartnerAspect]) { throw new Exception("Invalid partner token"); }
            if (props[Pid.PartnerToken] != tokenBase64Encoded) { throw new Exception("Invalid partner token"); }
        }

        private class RequestContext
        {
            public string user;
            public string item;
            public string expires;
        }

        private RequestContext GetContext(string tokenBase64Encoded)
        {
            var rc = new RequestContext();

            var tokenString = Tools.Base64.Decode(tokenBase64Encoded);
            var tokenNode = new JsonPath.Node(tokenString);
            var payloadNode = tokenNode["payload"];

            rc.user = payloadNode["user"];
            rc.item = payloadNode["item"];
            rc.expires = payloadNode["expires"];

            if (!Has.Value(rc.user)) { throw new Exception("No in context"); }
            if (!Has.Value(rc.item)) { throw new Exception("No item in context"); }

            return rc;
        }

    }
}