using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using n3q.Web.Models;
using n3q.GrainInterfaces;
using Microsoft.Extensions.Configuration;
using n3q.Tools;

namespace n3q.Web.Controllers
{
    [ApiController]
    public class ItemController : ControllerBase
    {
        public ICallbackLogger Log { get; set; }
        readonly WebConfig _config;

        public ItemController(ILogger<ItemController> logger, WebConfig config)
        {
            Log = new FrameworkCallbackLogger(logger);
            _config = config;
        }

        [Route("[controller]/Config")]
        [HttpGet]
        public async Task<ItemServiceConfig> Config(string id)
        {
            await Task.CompletedTask;

            if (string.IsNullOrEmpty(id)) { throw new Exception("No id"); }
            Log.Info(id, "Config", nameof(ItemController));

            var secretToken = "";

            if (string.IsNullOrEmpty(id)) { throw new Exception("No use token generated"); }

            var result = new ItemServiceConfig {
                serviceUrl = _config.ItemServiceXmppUrl,
                unavailableUrl = _config.WebBaseUrl + "Embedded/Account?id={id}",
                userToken = secretToken,
                itemPropertyUrlFilter = new Dictionary<string, string> {
                    //{ "{image.item.nine3q}", "https://nine3q.dev.sui.li/images/Items/" },
                    { "{image.item.nine3q}", _config.WebBaseUrl + "images/Items/" },
                },
            };
            return result;
        }
    }
}