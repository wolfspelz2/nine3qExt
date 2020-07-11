using Orleans;
using n3q.GrainInterfaces;
using System.Threading.Tasks;

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
    
    public class OrleansItemClusterClient: ItemClusterClientBase, IItemClusterClient
    {
        public readonly IClusterClient OrleansClusterClient;

        public OrleansItemClusterClient(IClusterClient clusterClient)
        {
            OrleansClusterClient = clusterClient;
        }

        public override IItemClient GetItemClient(string itemId)
        {
            return new OrleansClusterItemClient(OrleansClusterClient, itemId);
        }
    }
}
