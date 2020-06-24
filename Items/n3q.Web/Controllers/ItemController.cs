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
        readonly IConfiguration _configuration;

        public ItemController(ILogger<ItemController> logger, IConfiguration configuration)
        {
            Log = new FrameworkCallbackLogger(logger);
            _configuration = configuration;
        }

        [Route("[controller]/Config")]
        [HttpGet]
        public async Task<ItemServiceConfig> Get(string id)
        {
            await Task.CompletedTask;

            if (string.IsNullOrEmpty(id)) { throw new Exception("No id"); }
            Log.Info(id, nameof(Config), nameof(ItemController));

            var secretToken = "";

            if (string.IsNullOrEmpty(id)) { throw new Exception("No use token generated"); }

            var config = new ItemServiceConfig {
                serviceUrl = _configuration.GetValue(nameof(WebConfig.ItemServiceXmppUrl), "xmpp:itemsxmpp.dev.sui.li"),
                unavailableUrl = _configuration.GetValue(nameof(WebConfig.WebBaseUrl),  "http://localhost:5000/") + "Embedded/Account?id={id}",
                userToken = secretToken,
                itemPropertyUrlFilter = new Dictionary<string, string> {
                    //{ "{image.item.nine3q}", "https://nine3q.dev.sui.li/images/Items/" },
                    { "{image.item.nine3q}", _configuration.GetValue(nameof(WebConfig.WebBaseUrl),  "http://localhost:5000/") + "images/Items/" },
                },
            };
            return config;
        }
    }
}