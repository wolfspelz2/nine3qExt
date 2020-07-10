using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Orleans;
using n3q.Aspects;
using n3q.Tools;

namespace n3q.WebIt
{
    public class TheatreScreenplayDispenserModel : PageModel
    {
        public WebItConfigDefinition Config { get; set; }
        public IItemClusterClient ItemClient { get; set; }

        public TheatreScreenplayDispenserModel(WebItConfigDefinition config, IClusterClient clusterClient)
        {
            Config = config;
            ItemClient = new OrleansItemClusterClient(clusterClient);
        }

        public void OnGet(string context)
        {
            var ctx = ContextToken.FromBase64Token(Config.PayloadHashSecret, context);
        }

        public void OnPost(string context)
        {
            var ctx = ContextToken.FromBase64Token(Config.PayloadHashSecret, context);

            var action = HttpContext.Request.Form["action"].First();
            var args = HttpContext.Request.Form
                .Where(kv => kv.Key != "action")
                .ToStringDictionary(x => x.First())
                ;
        }
    }
}