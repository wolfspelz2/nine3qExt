using n3q.GrainInterfaces;

namespace n3q.Aspects
{
    public class SiloSimulatorItemClient : IItemClient
    {
        readonly string _id;
        readonly SiloSimulator _simulator;

        public string GetId() => _id;

        public SiloSimulatorItemClient(SiloSimulator simulator, string id)
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
            return new SiloSimulatorItemClient(_simulator, otherId);
        }
    }

    public class SiloSimulatorClusterClient : IItemClusterClient
    {
        readonly SiloSimulator _simulator;

        public SiloSimulatorClusterClient(SiloSimulator simulator)
        {
            _simulator = simulator;
        }

        public IItemClient ItemClient(string itemId)
        {
            return new SiloSimulatorItemClient(_simulator, itemId);
        }
    }
}
