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

        //[Route("[controller]")]
        //[HttpGet]
        //public async Task<IEnumerable<Sample>> Get()
        //{
        //    _logger.LogInformation("Get");
        //    var ids = new[] { "a", "b" };
        //    var samples = new List<Sample>();
        //    foreach (var id in ids) {
        //        samples.Add(new Sample {
        //            Key = id,
        //            Value = await _clusterClient.GetGrain<ITestString>(id).Get()
        //        });
        //    }
        //    return samples;
        //}

        /*
            {
              "config": {
                "serviceUrl": "https://config.weblin.sui.li/"
              },
              "inventory": {
                "iconSize": 64
              },
              "itemProviders": {
                "nine3q": {
                  "configUrl": "http://localhost:5000/Item/Config",
                  "config": {
                    "serviceUrl": "xmpp:itemsxmpp.dev.sui.li",
                    "userToken": "random-user-token-jhg2fu7kjjl4koi8tgi",
                    "itemPropertyUrlFilter": {
                      "{image.item.nine3q}": "http://localhost:5000/images/Items/"
                    }
                  }
                }
              }
            }
        */

        [Route("[controller]/Config")]
        [HttpGet]
        public async Task<ItemServiceConfig> Get()
        {
            await Task.CompletedTask;
            return new ItemServiceConfig {
                serviceUrl = "xmpp:itemsxmpp.dev.sui.li",
                accountUrl = "http://localhost:5000/Account",
                userToken = "random-user-token-jhg2fu7kjjl4koi8tgi",
                itemPropertyUrlFilter = new Dictionary<string, string> {
                    //{ "{image.item.nine3q}", "https://nine3q.dev.sui.li/images/Items/" },
                    { "{image.item.nine3q}", "http://localhost:5000/images/Items/" },
                },
            };
        }
    }
}