using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                switch (method) {
                    case nameof(Echo): response = Echo(request); break;
                    case nameof(ComputePayloadHash): response = ComputePayloadHash(request); break;
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
        public JsonPath.Dictionary ComputePayloadHash(JsonPath.Dictionary request)
        {
            var user = request["user"].String;
            var payloadBase64Encoded = request["payload"].String;

            if (!Has.Value(user)) { throw new Exception("No user"); }
            if (!Has.Value(payloadBase64Encoded)) { throw new Exception("No payload"); }

            var payloadBase64DecodedBytes = Convert.FromBase64String(payloadBase64Encoded);
            var payload = Encoding.UTF8.GetString(payloadBase64DecodedBytes);
            var json = new JsonPath.Node(payload);
            if (json["user"].String != user) { throw new Exception("User mismatch"); }

            var data = Config.PayloadHashSecret + payload;
            var hash = Tools.Crypto.SHA256Hex(data);

            return new JsonPath.Dictionary().Add("result", hash);
        }

        [Route("[controller]/{action}")]
        public async Task<Dictionary> GetItemProperties(JsonPath.Dictionary request)
        {
            var itemId = request["item"].String;
            if (!Has.Value(itemId)) { throw new Exception("No item"); }

            ValidateProviderHash();

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

        private void ValidateProviderHash()
        {
        }
    }
}