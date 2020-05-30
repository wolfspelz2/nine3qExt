using n3q.GrainInterfaces;
using n3q.Items;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace n3q.Aspects
{
    public class ItemGrainSimulator : IItem
    {
        public PropertySet Properties { get; set; }

        public ItemGrainSimulator()
        {
        }

        public Task ModifyProperties(PropertySet modified, PidSet deleted, Guid tid)
        {
            throw new NotImplementedException();
        }

        public Task AddToList(Pid pid, PropertyValue value, Guid tid)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromList(Pid pid, PropertyValue value, Guid tid)
        {
            throw new NotImplementedException();
        }

        public Task<PropertySet> GetProperties(PidSet pids, bool native = false)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> GetStreamId()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetStreamNamespace()
        {
            throw new NotImplementedException();
        }

        public Task Deactivate()
        {
            throw new NotImplementedException();
        }

        public Task WritePersistentStorage()
        {
            throw new NotImplementedException();
        }

        public Task ReadPersistentStorage()
        {
            throw new NotImplementedException();
        }

        public Task DeletePersistentStorage()
        {
            throw new NotImplementedException();
        }

        public Task BeginTransaction(Guid tid)
        {
            throw new NotImplementedException();
        }

        public Task EndTransaction(Guid tid, bool success)
        {
            throw new NotImplementedException();
        }

        public Task Delete(Guid tid)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemSiloSimulator
    {
        readonly Dictionary<string, ItemGrainSimulator> _grains = new Dictionary<string, ItemGrainSimulator>();

        internal IItem GetGrain(string id)
        {
            if (_grains.TryGetValue(id, out var grain)) {
                return grain;
            } else {
                grain = new ItemGrainSimulator();
                _grains[id] = grain;
                return grain;
            }
        }
    }
}
