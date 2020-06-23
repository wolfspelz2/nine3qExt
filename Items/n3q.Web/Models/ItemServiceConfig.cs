using System.Collections.Generic;

namespace n3q.Web.Models
{
    public class ItemServiceConfig
    {
        public string serviceUrl { get; set; }
        public string accountUrl { get; set; }
        public string userToken { get; set; }
        public Dictionary<string,string> itemPropertyUrlFilter { get; set; }
    }
}
