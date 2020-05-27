using Orleans;
using n3q.GrainInterfaces;
using System.Threading.Tasks;
using n3q.Items;
using System;

namespace n3q.Aspects
{
    public class Item : IItem
    {
        public IClusterClient ClusterClient { get; }
        public IGrainFactory GrainFactory { get; }
        public string Id { get; }

        public Item(IClusterClient clusterClient, string itemId)
        {
            ClusterClient = clusterClient;
            Id = itemId;
        }

        public Item(IGrainFactory grainFactory, string id)
        {
            GrainFactory = grainFactory;
            Id = id;
        }

        public IItem Grain
        {
            get {
                if (ClusterClient != null) {
                    return ClusterClient.GetGrain<IItem>(Id);
                } else if (GrainFactory != null) {
                    return GrainFactory.GetGrain<IItem>(Id);
                }
                throw new Exception($"Need valid IClusterClient or IGrainFactory for id={Id}");
            }
        }

        public Aspect AsAspect(Pid pid)
        {
            if (AspectRegistry.Aspects.ContainsKey(pid)) {
                return AspectRegistry.Aspects[pid](this);
            }
            throw new Exception($"Unknown pid/aspect={pid}");
        }


        #region IItem

        public Task Set(Pid pid, PropertyValue value) { return Grain.Set(pid, value); }
        public Task ModifyProperties(PropertySet modified, PidSet deleted) { return Grain.ModifyProperties(modified, deleted); }
        public Task<PropertySet> GetProperties(PidSet pids, bool native = false) { return Grain.GetProperties(pids, native); }
        public Task AddToItemSet(Pid pid, string itemId) { return Grain.AddToItemSet(pid, itemId); }
        public Task DeleteFromItemSet(Pid pid, string itemId) { return Grain.DeleteFromItemSet(pid, itemId); }

        public Task<Guid> GetStreamId() { return Grain.GetStreamId(); }
        public Task<string> GetStreamNamespace() { return Grain.GetStreamNamespace(); }
        public Task Deactivate() { return Grain.Deactivate(); }
        public Task WritePersistentStorage() { return Grain.WritePersistentStorage(); }
        public Task ReadPersistentStorage() { return Grain.ReadPersistentStorage(); }
        public Task DeletePersistentStorage() { return Grain.DeletePersistentStorage(); }

        #endregion

        #region IItem extensions

        //public async Task Set(Pid pid, PropertyValue value) { await Grain.ModifyProperties(new PropertySet(pid, value), PidSet.Empty); }

        public async Task Delete(Pid pid) { await Grain.ModifyProperties(PropertySet.Empty, new PidSet { pid }); }

        public async Task<PropertyValue> Get(Pid pid)
        {
            var props = await Grain.GetProperties(new PidSet { pid });
            if (props.ContainsKey(pid)) {
                return props[pid];
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
