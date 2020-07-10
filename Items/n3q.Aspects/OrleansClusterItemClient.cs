using Orleans;
using n3q.GrainInterfaces;

namespace n3q.Aspects
{
    public class OrleansClusterItemClient : IItemClient
    {
        readonly string _itemId;
        readonly IClusterClient _clusterClient;

        public string GetId() => _itemId;

        public OrleansClusterItemClient(IClusterClient clusterClient, string itemId)
        {
            _itemId = itemId;
            _clusterClient = clusterClient;
        }

        public IItem GetItem()
        {
            return _clusterClient.GetGrain<IItem>(_itemId);
        }

        public IItemClient CloneFor(string otherId)
        {
            return new OrleansClusterItemClient(_clusterClient, otherId);
        }
    }
    
    public class OrleansItemClusterClient: IItemClusterClient
    {
        readonly IClusterClient _clusterClient;

        public OrleansItemClusterClient(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        public IItemClient ItemClient(string itemId)
        {
            return new OrleansClusterItemClient(_clusterClient, itemId);
        }
    }
}
