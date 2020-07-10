using Orleans;
using n3q.GrainInterfaces;

namespace n3q.Aspects
{
    public class OrleansGrainFactoryItemClient : IItemClient
    {
        readonly string _grainId;
        readonly IGrainFactory _grainFactory;

        public string GetId() => _grainId;

        public OrleansGrainFactoryItemClient(IGrainFactory grainFactory, string grainId)
        {
            _grainId = grainId;
            _grainFactory = grainFactory;
        }

        public IItem GetItem()
        {
            return _grainFactory.GetGrain<IItem>(_grainId);
        }

        public IItemClient CloneFor(string otherId)
        {
            return new Aspects.OrleansGrainFactoryItemClient(_grainFactory, otherId);
        }
    }

    public class OrleansGrainFactoryClusterClient : ItemClusterClientBase, IItemClusterClient
    {
        public readonly IGrainFactory OrleansGrainFactory;

        public OrleansGrainFactoryClusterClient(IGrainFactory grainFactory)
        {
            OrleansGrainFactory = grainFactory;
        }

        public override IItemClient GetItemClient(string itemId)
        {
            return new OrleansGrainFactoryItemClient(OrleansGrainFactory, itemId);
        }
    }
}
