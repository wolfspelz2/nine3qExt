using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using n3q.Tools;
using n3q.Aspects;

namespace n3q.WebIt.Controllers
{
    [ApiController]
    public class IframeController : ControllerBase
    {
        public ICallbackLogger Log { get; set; }
        public WebItConfigDefinition Config { get; set; }
        public IItemClusterClient ItemClient { get; set; }

        public IframeController(ILogger<IframeController> logger, WebItConfigDefinition config, IClusterClient clusterClient)
        {
            Log = new FrameworkCallbackLogger(logger);
            Config = config;
            ItemClient = new OrleansItemClusterClient(clusterClient);
        }

        [Route("[controller]")]
        [HttpGet]
        public async Task<ContentResult> Get(string context)
        {
            await Task.CompletedTask;
            var ctx = ContextToken.FromBase64Token(Config.PayloadHashSecret, context);

            var html = @"
<html lang='en'>
<head>
    <meta charset='utf-8'>
    <title>title</title>
    <style>
    </style>
</head>
<body>

<form id='form' method='POST' target='hidden-form'>
    <input type='hidden' name='action' id='action' value='GetItem' />
    <input type='hidden' name='arg1' id='arg1' value='value1' />
    <button id='submit' onclick='document.getElementById(\'form\').post();' >GetItem</button>
</form>
<iframe style='display:none; witdth:10px; height:10px' name='hidden-form'></iframe>

</body>
</html>
";

            return new ContentResult() { Content = html, ContentType = "text/html" };
        }

        [Route("[controller]")]
        [HttpPost]
        public async Task<string> Post(string context)
        {
            await Task.CompletedTask;
            var ctx = ContextToken.FromBase64Token(Config.PayloadHashSecret, context);

            var action = HttpContext.Request.Form["action"].First();
            var args = HttpContext.Request.Form
                .Where(kv => kv.Key != "action")
                .ToStringDictionary(x => x.First())
                ;

            return "";
        }
    }
}