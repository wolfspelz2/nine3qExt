using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using n3q.Web.Models;
using n3q.GrainInterfaces;

namespace n3q.Web.Controllers
{
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly ILogger<ItemController> _logger;

        public ItemController(ILogger<ItemController> logger)
        {
            _logger = logger;
            _ = _logger;
        }

        [Route("[controller]/Config")]
        [HttpGet]
        public async Task<ItemServiceConfig> Get(string id)
        {
            await Task.CompletedTask;

            if (string.IsNullOrEmpty(id)) { throw new Exception("No id"); }

            var config = new ItemServiceConfig {
                serviceUrl = "xmpp:itemsxmpp.dev.sui.li",
                unavailableUrl = $"http://localhost:5000/Embedded/Account?id={id}",
                //userToken = "random-user-token-jhg2fu7kjjl4koi8tgi",
                itemPropertyUrlFilter = new Dictionary<string, string> {
                    //{ "{image.item.nine3q}", "https://nine3q.dev.sui.li/images/Items/" },
                    { "{image.item.nine3q}", "http://localhost:5000/images/Items/" },
                },
            };
            return config;
        }
    }
}