using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using n3q.Aspects;
using n3q.Tools;
using n3q.Items;

namespace n3q.WebIt.ItemFrame
{
    [IgnoreAntiforgeryToken(Order = 2000)]
    public class MagicHatModel : PageModel
    {
        public WebItConfigDefinition Config { get; set; }
        public IItemClusterClient ClusterClient { get; set; }

        public MagicHatModel(WebItConfigDefinition config, IClusterClient clusterClient)
        {
            Config = config;
            ClusterClient = new OrleansItemClusterClient(clusterClient);
        }

        public void OnGet(string context)
        {
            var ctx = ContextToken.FromBase64Token(Config.PayloadHashSecret, context);
        }

        public async Task OnPost(string context)
        {
            var ctx = ContextToken.FromBase64Token(Config.PayloadHashSecret, context);

            var action = HttpContext.Request.Form["action"].First();
            var template = HttpContext.Request.Form["template"].First();

            await ClusterClient.Transaction(ctx.ItemId, async self => {

                var newItem = await self.NewItemFromTemplate(template, ctx.UserId);

                var containerId = await self.GetItemId(Pid.Container);
                if (Has.Value(containerId)) {
                    var container = await self.WritableItem(containerId);
                    await container.AsContainer().AddChild(newItem);
                }

            });
        }
    }
}