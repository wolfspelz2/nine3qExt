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

        [Route("[controller]/Config")]
        [HttpGet]
        public async Task<ItemServiceConfig> Get()
        {
            await Task.CompletedTask;
            return new ItemServiceConfig {
                ServiceUrl = "xmpp:items.xmpp.dev.sui.li",
                ItemPropertyUrlFilter = new Dictionary<string, string> {
                    { "{image.item.nine3q}", "https://nine3q.dev.sui.li/images/Items/" },
                },
            };
        }
    }
}