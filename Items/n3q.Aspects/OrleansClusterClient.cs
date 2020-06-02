using Orleans;
using n3q.GrainInterfaces;

namespace n3q.Aspects
{
    public class OrleansClusterClient : IItemClient
    {
        readonly string _grainId;
        readonly IClusterClient _clusterClient;

        public string GetId() => _grainId;

        public OrleansClusterClient(IClusterClient clusterClient, string grainId)
        {
            _grainId = grainId;
            _clusterClient = clusterClient;
        }

        public IItem GetItem()
        {
            return _clusterClient.GetGrain<IItem>(_grainId);
        }

        public IItemClient CloneFor(string otherId)
        {
            return new OrleansClusterClient(_clusterClient, otherId);
        }
    }
}
