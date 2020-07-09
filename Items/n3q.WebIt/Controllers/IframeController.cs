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
        public async Task<string> Get(string context)
        {
            await Task.CompletedTask;

            var ctx = ContextToken.FromBase64TokenAndValiate(Config.PayloadHashSecret, context);

            return ctx.ItemId;
        }
    }
}