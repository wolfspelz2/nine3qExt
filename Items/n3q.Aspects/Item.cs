using Orleans;
using n3q.GrainInterfaces;
using System.Threading.Tasks;
using n3q.Items;
using System;
using System.Collections.Generic;

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
        public IClusterClient ClusterClient { get; }
        public IGrainFactory GrainFactory { get; }
        public ItemSiloSimulator Simulator { get; }
        public string Id { get; }
        public Transaction Transaction { get; }

        public ItemStub(IClusterClient clusterClient, string itemId, Transaction t)
        {
            ClusterClient = clusterClient;
            Id = itemId;
            Transaction = t;
        }

        public ItemStub(IGrainFactory grainFactory, string id, Transaction t)
        {
            GrainFactory = grainFactory;
            Id = id;
            Transaction = t;
        }

        public ItemStub(ItemSiloSimulator simulator, string id, Transaction t)
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

        public Aspect AsAspect(Pid pid)
        {
            if (AspectRegistry.Aspects.ContainsKey(pid)) {
                var aspect = AspectRegistry.Aspects[pid](this);
                return aspect;
            }
            throw new Exception($"Unknown pid/aspect={pid}");
        }


        #region IItem

        public Task ModifyProperties(PropertySet modified, PidSet deleted, Guid tid) { throw new NotImplementedException(); }
        public Task AddToList(Pid pid, PropertyValue value, Guid tid) { throw new NotImplementedException(); }
        public Task DeleteFromList(Pid pid, PropertyValue value, Guid tid) { throw new NotImplementedException(); }

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

        public Task ModifyProperties(PropertySet modified, PidSet deleted) { return Grain.ModifyProperties(modified, deleted, Transaction.Id); }
        public Task AddToList(Pid pid, PropertyValue value) { return Grain.AddToList(pid, value, Transaction.Id); }
        public Task DeleteFromList(Pid pid, PropertyValue value) { return Grain.DeleteFromList(pid, value, Transaction.Id); }
        public Task<PropertySet> GetProperties(PidSet pids, bool native = false) { return Grain.GetProperties(pids, native); }

        public async Task<Transaction> BeginTransaction()
        {
            await Grain.BeginTransaction(Transaction.Id);
            return Transaction;
        }
        public Task EndTransaction(bool success) { return Grain.EndTransaction(Transaction.Id, success); }

        public async Task Set(Pid pid, PropertyValue value) { await Grain.ModifyProperties(new PropertySet(pid, value), PidSet.Empty, Transaction.Id); }
        public async Task Delete(Pid pid) { await Grain.ModifyProperties(PropertySet.Empty, new PidSet { pid }, Transaction.Id); }

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

        #endregion

        //#region Aspects

        //public Container AsContainer => new Container(this);
        //public CapacityLimit AsCapacityLimit => new CapacityLimit(this);
        //public Rezable AsRezable => new Rezable(this);

        //#endregion
    }
}
