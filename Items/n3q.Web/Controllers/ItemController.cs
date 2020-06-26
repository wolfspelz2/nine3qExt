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

namespace n3q.Web.Controllers
{
    [ApiController]
    public class ItemController : ControllerBase
    {
        public ICallbackLogger Log { get; set; }
        public WebConfig Config { get; set; }
        public IClusterClient Cluster { get; set; }

        public ItemController(ILogger<ItemController> logger, WebConfig config, IClusterClient clusterClient)
        {
            Log = new FrameworkCallbackLogger(logger);
            Config = config;
            Cluster = clusterClient;
        }

        [Route("[controller]/Config")]
        [HttpGet]
        public async Task<ItemServiceConfig> Get(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new Exception("No id"); }

            var token = GetLowercaseTokenBecauseWillBeSentAsXmppUser(id);
            Log.Info(token, "Config", nameof(ItemController));

            var itemRef = Cluster.GetGrain<IItemRef>(token);
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
                    //{ "{image.item.nine3q}", "https://nine3q.dev.sui.li/images/Items/" },
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
            var tmpl = DevSpec.Template.Inventory;
            var itemId = $"{tmpl.ToString()}-{RandomString.GetAlphanumLowercase(20)}".ToLower();

            var item = Cluster.GetItemStub(itemId);
            await item.WithTransaction(async self => {
                await self.ModifyProperties(new PropertySet { [Pid.Template] = tmpl.ToString() }, PidSet.Empty);
            });

            await Cluster.GetGrain<IWorker>(Guid.Empty).AspectAction(itemId, Pid.InventoryAspect, nameof(Inventory.Action.Initialize), PropertySet.Empty);

            return itemId;
        }
    }
}