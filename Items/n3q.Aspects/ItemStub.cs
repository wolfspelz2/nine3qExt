using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using n3q.GrainInterfaces;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public interface IItemClient
    {
        string GetId();
        IItem GetItem();
        IItemClient CloneFor(string otherId);
    }

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

    public class ItemStub
    {
        public IItemClient Client;
        public ItemTransaction Transaction;

        public string Id => Client.GetId();

        public ItemStub(IItemClient itemClient, ItemTransaction transaction = null)
        {
            Client = itemClient;
            Transaction = transaction;
        }

        public IItem Grain => Client.GetItem();

        public async Task<ItemStub> Item(string itemId)
        {
            if (!Has.Value(itemId)) {
                throw new Exception($"{nameof(Aspect)}.{nameof(Item)}: Empty or null itemId");
            }

            var client = Client.CloneFor(itemId);
            var item = new ItemStub(client, Transaction);
            await Transaction?.AddItem(item);
            return item;
        }

        #region Aspects

        public async Task ForeachAspect(Action<Aspect> action)
        {
            foreach (var key in await GetAspects()) {
                var aspect = AsAspect(key);
                if (aspect != null) {
                    action(aspect);
                }
            }
        }

        public async Task<IEnumerable<Pid>> GetAspects()
        {
            var aspectProps = await GetProperties(PidSet.Aspects);
            var itemAspectPids = aspectProps.Keys;
            var knownAspectPids = AspectRegistry.Aspects.Keys;
            return itemAspectPids.Intersect(knownAspectPids);
        }

        public Aspect AsAspect(Pid pid)
        {
            if (AspectRegistry.Aspects.ContainsKey(pid)) {
                var aspect = AspectRegistry.Aspects[pid](this);
                return aspect;
            }
            throw new Exception($"Unknown pid/aspect={pid}");
        }

        #endregion

        #region IItem stubs

        public async Task ModifyProperties(PropertySet modified, PidSet deleted) { AssertTransaction(); await Grain.ModifyProperties(modified, deleted, Transaction.Id); }
        public async Task AddToList(Pid pid, PropertyValue value) { AssertTransaction(); await Grain.AddToListProperty(pid, value, Transaction.Id); }
        public async Task RemoveFromList(Pid pid, PropertyValue value) { AssertTransaction(); await Grain.RemoveFromListProperty(pid, value, Transaction.Id); }

        public async Task<PropertySet> GetProperties(PidSet pids, bool native = false)
        {
            var t = Transaction;
            var result = await Grain.GetPropertiesX(pids, native);
            if (Transaction == null) {
                Transaction = t;
            }
            return result;
        }

        public async Task BeginTransaction() { AssertTransaction(); await Grain.BeginTransaction(Transaction.Id); }
        public async Task EndTransaction(bool success) { AssertTransaction(); await Grain.EndTransaction(Transaction.Id, success); }

        public async Task Delete() { AssertTransaction(); await Grain.Delete(Transaction.Id); }

        public async Task Set(Pid pid, PropertyValue value) { AssertTransaction(); await Grain.ModifyProperties(new PropertySet(pid, value), PidSet.Empty, Transaction.Id); }
        public async Task Unset(Pid pid) { AssertTransaction(); await Grain.ModifyProperties(PropertySet.Empty, new PidSet { pid }, Transaction.Id); }

        public async Task<PropertyValue> Get(Pid pid)
        {
            var props = await GetProperties(new PidSet { pid });
            if (props.TryGetValue(pid, out var value)) {
                return value;
            }
            return PropertyValue.Empty;
        }

        public async Task<string> GetString(Pid pid) { return await Get(pid); }
        public async Task<long> GetInt(Pid pid) { return await Get(pid); }
        public async Task<double> GetFloat(Pid pid) { return await Get(pid); }
        public async Task<bool> GetBool(Pid pid) { return await Get(pid); }
        public async Task<string> GetItemId(Pid pid) { return await Get(pid); }
        public async Task<ValueList> GetItemIdList(Pid pid) { return await Get(pid); }
        public async Task<ValueList> GetList(Pid pid) { return await Get(pid); }
        public async Task<ValueMap> GetMap(Pid pid) { return await Get(pid); }

        public async Task Deactivate() { await Grain.Deactivate(); }
        public async Task WritePersistentStorage() { await Grain.WritePersistentStorage(); }
        public async Task ReadPersistentStorage() { await Grain.ReadPersistentStorage(); }
        public async Task DeletePersistentStorage() { await Grain.DeletePersistentStorage(); }

        public delegate Task TransactionWrappedCode(ItemStub item);
        public async Task WithTransaction(TransactionWrappedCode transactedCode)
        {
            Transaction = new ItemTransaction();
            await Transaction.Begin(this);
            try {
                await transactedCode(this);
                await Transaction.Commit();
            } catch (Exception ex) {
                _ = ex;
                await Transaction.Cancel();
                throw;
            } finally {
                Transaction = null;
            }
        }

        private void AssertTransaction()
        {
            if (Transaction == null) {
                throw new Exception("No transaction");
            }
        }

        private void AssertStubMethodIsUsed()
        {
            throw new Exception($"Do not use the interface directly. Please use the stub method {nameof(GetProperties)}");
        }

        #endregion
    }
}
