using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using n3q.GrainInterfaces;
using n3q.Items;

namespace n3q.Aspects
{
    public class SiloSimulatorItem : IItem
    {
        public PropertySet Properties { get; set; } = new PropertySet();

        public SiloSimulatorItem()
        {
        }

        public Task ModifyProperties(PropertySet modified, PidSet deleted, Guid tid)
        {
            foreach (var pair in modified) { Properties[pair.Key] = pair.Value; }
            foreach (var pid in deleted) { if (Properties.ContainsKey(pid)) { Properties.Remove(pid); } }
            return Task.CompletedTask;
        }

        public Task AddToListProperty(Pid pid, PropertyValue value, Guid tid)
        {
            var vl = Properties.GetItemIdSet(pid);
            vl.Add(value);
            Properties[pid] = vl;
            return Task.CompletedTask;
        }

        public Task RemoveFromListProperty(Pid pid, PropertyValue value, Guid tid)
        {
            throw new NotImplementedException();
            //return Task.CompletedTask;
        }

        public Task<PropertySet> GetProperties(PidSet pids, bool native = false)
        {
            var result = new PropertySet();
                foreach (var pair in Properties) {
                    if (pids.Contains(pair.Key)
                        || pids.Contains(Pid.MetaAspectGroup) && Property.GetDefinition(pair.Key).Group == Property.Group.Aspect
                        ) {
                        result.Add(pair.Key, pair.Value);
                    }
                }
            return Task.FromResult(result);
        }

        public Task<Guid> GetStreamId()
        {
            throw new NotImplementedException();
            //return Task.CompletedTask;
        }

        public Task<string> GetStreamNamespace()
        {
            throw new NotImplementedException();
            //return Task.CompletedTask;
        }

        public Task Deactivate()
        {
            throw new NotImplementedException();
            //return Task.CompletedTask;
        }

        public Task DeletePersistentStorage()
        {
            throw new NotImplementedException();
            //return Task.CompletedTask;
        }

        public Task BeginTransaction(Guid tid)
        {
            throw new NotImplementedException();
            //return Task.CompletedTask;
        }

        public Task EndTransaction(Guid tid, bool success)
        {
            throw new NotImplementedException();
            //return Task.CompletedTask;
        }

        public Task Delete(Guid tid)
        {
            throw new NotImplementedException();
            //return Task.CompletedTask;
        }
    }

    public class SiloSimulator
    {
        public Dictionary<string, SiloSimulatorItem> Items = new Dictionary<string, SiloSimulatorItem>();

        internal IItem GetGrain(string id)
        {
            if (Items.TryGetValue(id, out var grain)) {
                return grain;
            } else {
                grain = new SiloSimulatorItem();
                Items[id] = grain;
                return grain;
            }
        }
    }
}
