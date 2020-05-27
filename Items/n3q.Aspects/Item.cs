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

        public Task Set(Pid pid, string value) { return Grain.Set(pid, value); }
        public Task Set(Pid pid, long value) { return Grain.Set(pid, value); }
        public Task Set(Pid pid, double value) { return Grain.Set(pid, value); }
        public Task Set(Pid pid, bool value) { return Grain.Set(pid, value); }
        public Task Set(Pid pid, ItemIdSet value) { return Grain.Set(pid, value); }
        public Task AddToItemSet(Pid pid, string itemId) { return Grain.AddToItemSet(pid, itemId); }
        public Task DeleteFromItemSet(Pid pid, string itemId) { return Grain.DeleteFromItemSet(pid, itemId); }

        public Task<PropertyValue> Get(Pid pid) { return Grain.Get(pid); }
        public Task<string> GetString(Pid pid) { return Grain.GetString(pid); }
        public Task<long> GetInt(Pid pid) { return Grain.GetInt(pid); }
        public Task<double> GetFloat(Pid pid) { return Grain.GetFloat(pid); }
        public Task<bool> GetBool(Pid pid) { return Grain.GetBool(pid); }
        public Task<string> GetItemId(Pid pid) { return Grain.GetItemId(pid); }
        public Task<ItemIdSet> GetItemIdSet(Pid pid) { return Grain.GetItemIdSet(pid); }
        public Task<PropertySet> GetProperties(PidSet pids, bool native = false) { return Grain.GetProperties(pids, native); }

        public Task Delete(Pid pid) { return Grain.Delete(pid); }

        public Task<Guid> GetStreamId() { return Grain.GetStreamId(); }
        public Task<string> GetStreamNamespace() { return Grain.GetStreamNamespace(); }
        public Task Deactivate() { return Grain.Deactivate(); }
        public Task WritePersistentStorage() { return Grain.WritePersistentStorage(); }
        public Task ReadPersistentStorage() { return Grain.ReadPersistentStorage(); }
        public Task DeletePersistentStorage() { return Grain.DeletePersistentStorage(); }

        #endregion

        //#region Aspects

        //public Container AsContainer => new Container(this);
        //public CapacityLimit AsCapacityLimit => new CapacityLimit(this);
        //public Rezable AsRezable => new Rezable(this);

        //#endregion
    }
}
