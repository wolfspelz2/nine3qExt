using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using JsonPath;

using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Orleans;

namespace n3q.Web.Controllers
{
    [ApiController]
    public class RpcController : ControllerBase
    {
        public WebConfigDefinition Config { get; set; }

        public RpcController(WebConfigDefinition config)
        {
            Config = config;
        }

        [Route("[controller]")]
        [HttpPost]
        public async Task<string> PostAsync()
        {
            var response = new JsonPath.Dictionary();

            try {
                var body = await new StreamReader(Request.Body).ReadToEndAsync();

                var request = new JsonPath.Node(body);

                var method = request.AsDictionary["method"].AsString;
                switch (method) {
                    case "echo": response = Echo(request); break;
                    case "computePayloadHash": response = ComputePayloadHash(request); break;
                    default: throw new Exception($"Unknown method={method}");
                }

                response["status"] = "ok";

            } catch (Exception ex) {
                response["status"] = "error";
                response["message"] = ex.Message;
            }

            return response.ToNode().ToJson();
        }

        private JsonPath.Dictionary Echo(JsonPath.Node request)
        {
            return request.AsDictionary
                .Select(pair => new KeyValuePair<string, Node>(pair.Key, new JsonPath.Node(Node.Type.Auto, pair.Value)))
                .ToDictionary()
                ;
        }

        private JsonPath.Dictionary ComputePayloadHash(JsonPath.Node request)
        {
            return new JsonPath.Dictionary().Add("result", "xx");
        }
    }
}