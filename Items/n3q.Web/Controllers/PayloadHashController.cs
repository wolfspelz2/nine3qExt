using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace n3q.Web.Controllers
{
    [ApiController]
    public class PayloadHashController : ControllerBase
    {
        public WebConfigDefinition Config { get; set; }

        public PayloadHashController(WebConfigDefinition config)
        {
            Config = config;
        }

        [Route("[controller]")]
        [HttpGet]
        public string Get(string user, string payload)
        {
            if (string.IsNullOrEmpty(user)) { throw new Exception("No user"); }
            if (string.IsNullOrEmpty(payload)) { throw new Exception("No payload"); }

            var payloadBase64Decoded = Convert.FromBase64String(payload);
            var payloadBase64DecodedString = Encoding.UTF8.GetString(payloadBase64Decoded);
            var json = new JsonPath.Node(payloadBase64DecodedString);
            if (json["user"].String != user) { throw new Exception("User mismatch"); }

            var data = Config.PayloadHashSecret + payloadBase64DecodedString;
            var hash = Tools.Crypto.SHA256Hex(data);

            return hash;
        }
    }
}