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
using n3q.Common;

namespace n3q.WebIt.Controllers
{
    [ApiController]
    public partial class RpcController : ControllerBase
    {
        public ICallbackLogger Log { get; set; }
        public WebItConfigDefinition Config { get; set; }
        public IItemClusterClient ClusterClient { get; set; }

        public RpcController(ILogger<RpcController> logger, WebItConfigDefinition config, IClusterClient clusterClient)
        {
            Log = new FrameworkCallbackLogger(logger);
            Config = config;
            ClusterClient = new OrleansItemClusterClient(clusterClient);
        }

        //public RpcController(WebItConfigDefinition config, SiloSimulator siloSimulator)
        //{
        //    Log = new NullCallbackLogger();
        //    Config = config;
        //    ItemClient = new SiloSimulatorClusterClient(siloSimulator);
        //}

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

                var method = request[nameof(Protocol.Rpc.Request.method)].AsString.Capitalize();

                Log.Info(method);

                switch (method) {
                    case nameof(Echo): response = Echo(request); break;
                    case nameof(GetPayloadHash): response = GetPayloadHash(request); break;
                    case nameof(GetItemProperties): response = await GetItemProperties(request); break;
                    case nameof(ExecuteItemAction): response = await ExecuteItemAction(request); break;
                    default: throw new Exception($"Unknown method={method}");
                }

                response[nameof(Protocol.Rpc.Response.status)] = Protocol.Rpc.Response.status_ok;

            } catch (Exception ex) {
                response[nameof(Protocol.Rpc.Response.status)] = Protocol.Rpc.Response.status_error;
                response[nameof(Protocol.Rpc.Response.message)] = ex.Message;
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
            var payloadNode = request[nameof(Protocol.ContextToken.payload)];
            if (payloadNode.AsDictionary.Count == 0) { throw new Exception("No payload"); }

            var hash = Common.Protocol.ComputePayloadHash(Config.PayloadHashSecret, payloadNode);

            return new JsonPath.Dictionary().Add(nameof(Protocol.Rpc.Response.result), hash);
        }

        [Route("[controller]/{action}")]
        public async Task<Dictionary> GetItemProperties(JsonPath.Dictionary request)
        {
            var developerToken = request[nameof(Protocol.Rpc.GetItemPropertiesRequest.developer)].AsString;
            var developerId = await GetDeveloperIdAndValidateTheDeveloperToken(developerToken);

            var contextToken = request[nameof(Protocol.Rpc.GetItemPropertiesRequest.context)].AsString;
            var context = ContextToken.FromBase64Token(Config.PayloadHashSecret, contextToken);

            var pids = new PidSet(request[nameof(Protocol.Rpc.GetItemPropertiesRequest.pids)].AsList
                .Select(pidNode => pidNode.AsString.Capitalize().ToEnum(Pid.Unknown))
                .Where(pid => Property.GetDefinition(pid).Access == Property.Access.Public)
            );
            pids.Add(Pid.Developer);

            var itemReader = ClusterClient.GetItemReader(context.ItemId);
            var props = await itemReader.Get(pids);

            if (props.GetString(Pid.Developer) != developerId) { throw new Exception("Developer invalid"); }

            var propsNode = new Node(Node.Type.Dictionary);
            foreach (var pair in props) {
                propsNode.AsDictionary.Add(pair.Key.ToString(), pair.Value.ToString());
            }

            var response = new JsonPath.Dictionary {
                { nameof(Protocol.Rpc.Response.result), propsNode }
            };
            return response;
        }

        [Route("[controller]/{action}")]
        public async Task<Dictionary> ExecuteItemAction(JsonPath.Dictionary request)
        {
            var developerToken = request[nameof(Protocol.Rpc.ExecuteItemActionRequest.developer)].AsString;
            var developerId = await GetDeveloperIdAndValidateTheDeveloperToken(developerToken);

            var contextToken = request[nameof(Protocol.Rpc.ExecuteItemActionRequest.context)].AsString;
            var context = ContextToken.FromBase64Token(Config.PayloadHashSecret, contextToken);

            var action = request[nameof(Protocol.Rpc.ExecuteItemActionRequest.action)].AsString.Capitalize();
            if (!Has.Value(action)) { throw new Exception("No action"); }

            var args = request[nameof(Protocol.Rpc.ExecuteItemActionRequest.args)]
                .AsDictionary
                .ToStringDictionary(n => n.AsString)
                ;

            var itemWriter = ClusterClient.GetItemWriter(context.ItemId);

            var propDeveloperId = await itemWriter.GetItemId(Pid.Developer);
            if (propDeveloperId != developerId) { throw new Exception("Developer mismatch"); }

            await itemWriter.WithTransaction(async self => {
                await self.Execute(action, args);
            });

            var response = new JsonPath.Dictionary {
            };
            return response;
        }

        [Route("[controller]/{action}")]
        public async Task<string> GetDeveloperIdAndValidateTheDeveloperToken(string tokenBase64Encoded)
        {
            if (!Has.Value(tokenBase64Encoded)) { throw new Exception("No developer token"); }

            var tokenString = tokenBase64Encoded.FromBase64();
            var tokenNode = new JsonPath.Node(tokenString);
            var payloadNode = tokenNode[nameof(Protocol.DeveloperToken.payload)];

            var developerId = payloadNode[nameof(Protocol.DeveloperToken.Payload.developer)].AsString;
            if (!Has.Value(developerId)) { throw new Exception("No developer id in developer token"); }

            var itemReader = ClusterClient.GetItemReader(developerId);
            var props = await itemReader.Get(new PidSet { Pid.DeveloperAspect, Pid.DeveloperToken });
            if (!props[Pid.DeveloperAspect]) { throw new Exception("Invalid developer token"); }
            if (props[Pid.DeveloperToken] != tokenBase64Encoded) { throw new Exception("Invalid developer token"); }

            return developerId;
        }

    }
}