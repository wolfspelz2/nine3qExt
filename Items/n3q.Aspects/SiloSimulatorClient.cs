using n3q.GrainInterfaces;

namespace n3q.Aspects
{
    public class SiloSimulatorClient : IItemClient
    {
        readonly string _id;
        readonly ItemSiloSimulator _simulator;

        public string GetId() => _id;

        public SiloSimulatorClient(ItemSiloSimulator simulator, string id)
        {
            _id = id;
            _simulator = simulator;
        }

        public IItem GetItem()
        {
            return _simulator.GetGrain(_id);
        }

        public IItemClient CloneFor(string otherId)
        {
            return new SiloSimulatorClient(_simulator, otherId);
        }
    }
}
