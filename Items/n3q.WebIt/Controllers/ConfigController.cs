using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using n3q.WebIt.Models;
using n3q.GrainInterfaces;
using n3q.Tools;
using n3q.Items;
using n3q.Content;
using n3q.Aspects;
using n3q.Common;

namespace n3q.WebIt.Controllers
{
    [ApiController]
    public class ConfigController : ControllerBase
    {
        public ICallbackLogger Log { get; set; }
        public WebItConfigDefinition Config { get; set; }
        public OrleansItemClusterClient ClusterClient { get; set; }

        public ConfigController(ILogger<ConfigController> logger, WebItConfigDefinition config, IClusterClient clusterClient)
        {
            Log = new FrameworkCallbackLogger(logger);
            Config = config;
            ClusterClient = new OrleansItemClusterClient(clusterClient);
        }

        [Route("[controller]")]
        [HttpGet]
        public async Task<ItemServiceConfig> Get(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new Exception("No id"); }

            var token = GetLowercaseTokenBecauseWillBeSentAsXmppUser(id);
            Log.Info(token, "Config", nameof(ConfigController));

            var itemRef = ClusterClient.OrleansClusterClient.GetGrain<IItemRef>(token);
            var itemId = await itemRef.GetItem();
            if (string.IsNullOrEmpty(itemId)) {
                itemId = await CreateInventory();
                await itemRef.SetItem(itemId);
            }

            var result = new ItemServiceConfig {
                serviceUrl = Config.ItemServiceXmppUrl,
                apiUrl = Config.ItemServiceWebApiUrl,
                //unavailableUrl = Config.UnavailableUrl,
                userToken = token,
                itemPropertyUrlFilter = new Dictionary<string, string> {
                    { ItemService.ItemBaseVar, Config.ItemAppearanceBaseUrl },
                    { ItemService.ItemIframeVar, Config.ItemIframeBaseUrl },
                },
            };
            return result;
        }

        private static string GetLowercaseTokenBecauseWillBeSentAsXmppUser(string id)
        {
            return id.ToLower();
        }

        private async Task<string> CreateInventory()
        {
            var tmpl = DevSpec.Template.Inventory.ToString();
            var shortTmpl = tmpl.Substring(0, Cluster.LengthOfItemIdPrefixFromTemplate);
            var itemId = $"{shortTmpl}{RandomString.GetAlphanumLowercase(20)}";
            itemId = itemId.ToLower();
            var item = ClusterClient.GetItemWriter(itemId);

            await item.WithTransaction(async self => {
                await self.Modify(new PropertySet { [Pid.Template] = tmpl }, PidSet.Empty);
            });

            await ClusterClient.OrleansClusterClient.GetGrain<IWorker>(Guid.Empty).AspectAction(itemId, Pid.InventoryAspect, nameof(Inventory.Initialize), PropertySet.Empty);

            return itemId;
        }
    }
}