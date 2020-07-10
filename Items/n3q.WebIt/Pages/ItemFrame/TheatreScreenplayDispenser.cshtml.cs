using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using n3q.Aspects;
using n3q.Tools;

namespace n3q.WebIt.ItemFrame
{
    [IgnoreAntiforgeryToken(Order = 2000)]
    public class TheatreScreenplayDispenserModel : PageModel
    {
        public WebItConfigDefinition Config { get; set; }
        public IItemClusterClient ClusterClient { get; set; }

        public TheatreScreenplayDispenserModel(WebItConfigDefinition config, IClusterClient clusterClient)
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
            var args = HttpContext.Request.Form
                .Where(kv => kv.Key != "action")
                .ToStringDictionary(x => x.First())
                ;

            var itemWriter = new ItemWriter(ClusterClient.ItemClient(ctx.ItemId));

            await itemWriter.WithTransaction(async self => {
                await self.Execute(action, args);
            });
        }
    }
}