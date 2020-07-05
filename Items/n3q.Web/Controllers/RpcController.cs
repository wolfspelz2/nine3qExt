﻿using System;
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

                var request = new JsonPath.Node(body).AsDictionary;

                var method = request["method"].AsString;
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

        public JsonPath.Dictionary Echo(JsonPath.Dictionary request)
        {
            return request
                .Select(pair => new KeyValuePair<string, Node>(pair.Key, new JsonPath.Node(Node.Type.Auto, pair.Value)))
                .ToDictionary()
                ;
        }

        public JsonPath.Dictionary ComputePayloadHash(JsonPath.Dictionary request)
        {
            var user = request["user"].String;
            var payloadBase64Encoded = request["payload"].String;

            if (string.IsNullOrEmpty(user)) { throw new Exception("No user"); }
            if (string.IsNullOrEmpty(payloadBase64Encoded)) { throw new Exception("No payload"); }

            var payloadBase64DecodedBytes = Convert.FromBase64String(payloadBase64Encoded);
            var payload = Encoding.UTF8.GetString(payloadBase64DecodedBytes);
            var json = new JsonPath.Node(payload);
            if (json["user"].String != user) { throw new Exception("User mismatch"); }

            var data = Config.PayloadHashSecret + payload;
            var hash = Tools.Crypto.SHA256Hex(data);

            return new JsonPath.Dictionary().Add("result", hash);
        }
    }
}