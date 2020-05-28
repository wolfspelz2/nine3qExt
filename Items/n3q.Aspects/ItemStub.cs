using Orleans;
using n3q.GrainInterfaces;
using System.Threading.Tasks;
using n3q.Items;
using System;
using System.Collections.Generic;
using n3q.Tools;

namespace n3q.Aspects
{
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

        public Task DeleteFromList(Pid pid, PropertyValue value, Guid tid)
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
    }

    public class ItemStub : IItem
    {
        public string Id { get; }

        public IClusterClient ClusterClient { get; }
        public IGrainFactory GrainFactory { get; }
        public ItemSiloSimulator Simulator { get; }
        public ItemTransaction Transaction { get; set; }

        public ItemStub(IClusterClient clusterClient, string itemId, ItemTransaction t = null)
        {
            ClusterClient = clusterClient;
            Id = itemId;
            Transaction = t;
        }

        public ItemStub(IGrainFactory grainFactory, string id, ItemTransaction t = null)
        {
            GrainFactory = grainFactory;
            Id = id;
            Transaction = t;
        }

        public ItemStub(ItemSiloSimulator simulator, string id, ItemTransaction t = null)
        {
            Simulator = simulator;
            Id = id;
            Transaction = t;
        }

        public IItem Grain
        {
            get {
                if (ClusterClient != null) {
                    return ClusterClient.GetGrain<IItem>(Id);
                } else if (GrainFactory != null) {
                    return GrainFactory.GetGrain<IItem>(Id);
                } else if (Simulator != null) {
                    return Simulator.GetGrain(Id);
                }
                throw new Exception($"Need valid IClusterClient or IGrainFactory for id={Id}");
            }
        }

        public async Task<ItemStub> Item(string itemId)
        {
            if (!Has.Value(itemId)) {
                throw new Exception($"{nameof(Aspect)}.{nameof(Item)}: Empty or null itemId");
            }

            var item = (ItemStub)null;
            if (ClusterClient != null) {
                item = new ItemStub(ClusterClient, itemId, Transaction);
            } else if (GrainFactory != null) {
                item = new ItemStub(GrainFactory, itemId, Transaction);
            } else if (Simulator != null) {
                item = new ItemStub(Simulator, itemId, Transaction);
            }
            if (item != null) {
                await Transaction.AddItem(item);
                return item;
            } else {
                throw new Exception($"Need valid IClusterClient or IGrainFactory for id={Id}");
            }
        }

        public Aspect AsAspect(Pid pid)
        {
            if (AspectRegistry.Aspects.ContainsKey(pid)) {
                var aspect = AspectRegistry.Aspects[pid](this);
                return aspect;
            }
            throw new Exception($"Unknown pid/aspect={pid}");
        }


        #region IItem

        public Task ModifyProperties(PropertySet modified, PidSet deleted, Guid tid) { return Grain.ModifyProperties(modified, deleted, tid); }
        public Task AddToList(Pid pid, PropertyValue value, Guid tid) { return Grain.AddToList(pid, value, tid); }
        public Task DeleteFromList(Pid pid, PropertyValue value, Guid tid) { return Grain.DeleteFromList(pid, value, tid); }

        public Task BeginTransaction(Guid tid) { return Grain.BeginTransaction(tid); }
        public Task EndTransaction(Guid tid, bool success) { return Grain.EndTransaction(tid, success); }

        public Task<Guid> GetStreamId() { return Grain.GetStreamId(); }
        public Task<string> GetStreamNamespace() { return Grain.GetStreamNamespace(); }
        public Task Deactivate() { return Grain.Deactivate(); }
        public Task WritePersistentStorage() { return Grain.WritePersistentStorage(); }
        public Task ReadPersistentStorage() { return Grain.ReadPersistentStorage(); }
        public Task DeletePersistentStorage() { return Grain.DeletePersistentStorage(); }

        #endregion

        #region IItem extensions

        public Task ModifyProperties(PropertySet modified, PidSet deleted) { AssertTransaction(); return Grain.ModifyProperties(modified, deleted, Transaction.Id); }

        public Task AddToList(Pid pid, PropertyValue value) { AssertTransaction(); return Grain.AddToList(pid, value, Transaction.Id); }
        public Task DeleteFromList(Pid pid, PropertyValue value) { AssertTransaction(); return Grain.DeleteFromList(pid, value, Transaction.Id); }
        public Task<PropertySet> GetProperties(PidSet pids, bool native = false) { return Grain.GetProperties(pids, native); }

        public delegate Task TransactionWrappedCode(ItemStub item);
        public async Task WithTransaction(TransactionWrappedCode code)
        {
            Transaction = new ItemTransaction();
            await Transaction.Begin(this);
            try {
                await code(this);
                await Transaction.Commit();
            } catch (Exception ex) {
                _ = ex;
                await Transaction.Cancel();
                throw;
            } finally {
                Transaction = null;
            }
        }
        public Task EndTransaction(bool success) { AssertTransaction(); return Grain.EndTransaction(Transaction.Id, success); }

        public async Task Set(Pid pid, PropertyValue value) { AssertTransaction(); await Grain.ModifyProperties(new PropertySet(pid, value), PidSet.Empty, Transaction.Id); }
        public async Task Delete(Pid pid) { AssertTransaction(); await Grain.ModifyProperties(PropertySet.Empty, new PidSet { pid }, Transaction.Id); }

        public async Task<PropertyValue> Get(Pid pid)
        {
            var props = await Grain.GetProperties(new PidSet { pid });
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
        public async Task<ItemIdSet> GetItemIdSet(Pid pid) { return await Get(pid); }

        private void AssertTransaction()
        {
            if (Transaction == null) {
                throw new Exception("No transaction");
            }
        }

        #endregion

        //#region Aspects

        //public Container AsContainer => new Container(this);
        //public CapacityLimit AsCapacityLimit => new CapacityLimit(this);
        //public Rezable AsRezable => new Rezable(this);

        //#endregion
    }
}
