using System.Threading.Tasks;
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

    public class SiloSimulatorClusterClient : ItemClusterClientBase, IItemClusterClient
    {
        public readonly SiloSimulator Simulator;

        public SiloSimulatorClusterClient(SiloSimulator simulator)
        {
            Simulator = simulator;
        }

        public override IItemClient GetItemClient(string itemId)
        {
            return new SiloSimulatorItemClient(Simulator, itemId);
        }
    }
}
