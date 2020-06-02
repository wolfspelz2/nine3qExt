using Orleans;
using n3q.GrainInterfaces;

namespace n3q.Aspects
{
    public class OrleansGrainFactoryClient : IItemClient
    {
        readonly string _grainId;
        readonly IGrainFactory _grainFactory;

        public string GetId() => _grainId;

        public OrleansGrainFactoryClient(IGrainFactory grainFactory, string grainId)
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
            return new OrleansGrainFactoryClient(_grainFactory, otherId);
        }
    }
}
