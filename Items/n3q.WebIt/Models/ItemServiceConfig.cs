using System.Collections.Generic;

namespace n3q.WebIt.Models
{
    public class ItemServiceConfig
    {
        public string serviceUrl { get; set; }
        public string apiUrl { get; set; }
        //public string unavailableUrl { get; set; }
        public string userToken { get; set; }
        public Dictionary<string,string> itemPropertyUrlFilter { get; set; }
    }
}
