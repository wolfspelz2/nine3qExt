using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using n3q.Web.Models;
using n3q.GrainInterfaces;
using n3q.Tools;
using n3q.Items;
using n3q.Content;
using n3q.Aspects;
using n3q.Common;

namespace n3q.Web.Controllers
{
    [ApiController]
    public class ItemController : ControllerBase
    {
        public ICallbackLogger Log { get; set; }
        public WebConfigDefinition Config { get; set; }
        public IClusterClient ClusterClient { get; set; }

        public ItemController(ILogger<ItemController> logger, WebConfigDefinition config, IClusterClient clusterClient)
        {
            Log = new FrameworkCallbackLogger(logger);
            Config = config;
            ClusterClient = clusterClient;
        }

        [Route("[controller]/Config")]
        [HttpGet]
        public async Task<ItemServiceConfig> Get(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new Exception("No id"); }

            var token = GetLowercaseTokenBecauseWillBeSentAsXmppUser(id);
            Log.Info(token, "Config", nameof(ItemController));

            var itemRef = ClusterClient.GetGrain<IItemRef>(token);
            var itemId = await itemRef.GetItem();
            if (string.IsNullOrEmpty(itemId)) {
                itemId = await CreateInventory();
                await itemRef.SetItem(itemId);
            }

            var result = new ItemServiceConfig {
                serviceUrl = Config.ItemServiceXmppUrl,
                unavailableUrl = Config.UnavailableUrl,
                userToken = token,
                itemPropertyUrlFilter = new Dictionary<string, string> {
                    { "{image.item.nine3q}", Config.ItemBaseUrl },
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
            var item = ClusterClient.GetItemStub(itemId);

            await item.WithTransaction(async self => {
                await self.ModifyProperties(new PropertySet { [Pid.Template] = tmpl }, PidSet.Empty);
            });

            await ClusterClient.GetGrain<IWorker>(Guid.Empty).AspectAction(itemId, Pid.InventoryAspect, nameof(Inventory.Action.Initialize), PropertySet.Empty);

            return itemId;
        }
    }
}