using Microsoft.AspNetCore.Mvc.RazorPages;
using Orleans;
using n3q.GrainInterfaces;

namespace n3q.Web.Pages
{
    public class CounterModel : PageModel
    {
        private readonly IClusterClient _clusterClient;

        public long Value { get; set; }

        public CounterModel(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        public async System.Threading.Tasks.Task OnGet()
        {
            Value = await _clusterClient.GetGrain<ITestCounter>("TestCounter42").Get();
        }
    }
}