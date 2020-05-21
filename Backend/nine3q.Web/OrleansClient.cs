using System;
using Orleans;

namespace nine3q.Web
{
    public class OrleansClient : IOrleansClientSingletonInstance
    {
        public OrleansClient()
        {
        }

        public IClusterClient ClusterClient { get; set; }
    }
}