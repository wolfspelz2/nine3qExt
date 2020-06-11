using System;
using System.Collections.Generic;

namespace n3q.Web.Models
{
    public class ItemServiceConfig
    {
        public string ServiceUrl { get; set; }
        public string UserToken { get; set; }
        public Dictionary<string,string> ItemPropertyUrlFilter { get; set; }
    }
}
