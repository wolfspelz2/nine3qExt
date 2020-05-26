using Orleans;
using n3q.GrainInterfaces;

namespace n3q.Aspects
{
    public class Item
    {
        public Item(IClusterClient clusterClient, string itemId)
        {
            ClusterClient = clusterClient;
            Id = itemId;
        }

        public IClusterClient ClusterClient { get; }
        public string Id { get; }
        public IItem Grain => ClusterClient.GetGrain<IItem>(Id);
        public IItem I => Grain;
    }
}
