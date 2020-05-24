using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using nine3q.Web.Models;
using nine3q.GrainInterfaces;

namespace nine3q.Web.Controllers
{
    [ApiController]
    public class SampleController : ControllerBase
    {
        private readonly ILogger<SampleController> _logger;
        private readonly IClusterClient _clusterClient;

        public SampleController(ILogger<SampleController> logger, IClusterClient clusterClient)
        {
            _logger = logger;
            _clusterClient = clusterClient;
        }

        [Route("[controller]")]
        [HttpGet]
        public async Task<IEnumerable<Sample>> Get()
        {
            _logger.LogInformation("Get");
            var ids = new[] { "a", "b" };
            var samples = new List<Sample>();
            foreach (var id in ids) {
                samples.Add(new Sample {
                    Key = id,
                    Value = await _clusterClient.GetGrain<ITestString>(id).Get()
                });
            }
            return samples;
        }

        [Route("[controller]/{id}")]
        [HttpGet]
        public async Task<Sample> Get(string id)
        {
            _logger.LogInformation($"Get {id}");
            return new Sample {
                Key = id,
                Value = await _clusterClient.GetGrain<ITestString>(id).Get()
            };
        }

    }
}