using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using nine3q.Items;

namespace nine3q.GrainInterfaces
{
    public interface IInventory : IGrainWithStringKey
    {
        Task<long> CreateItem(PropertySet properties);
        Task<bool> DeleteItem(long id);

        Task SetItemProperties(long id, PropertySet properties);
        Task<PropertySet> GetItemProperties(long id, PidList pids, bool native = false);

        Task<long> GetItemByName(string name);
        Task<ItemIdSet> GetItems();
        Task<ItemIdPropertiesCollection> GetItemIdsAndValuesByProperty(Pid filterProperty, PidList desiredProperties);

        Task AddChildToContainer(long id, long containerId, long slot);
        Task RemoveChildFromContainer(long id, long containerId);

        Task<ItemIdPropertiesCollection> BeginItemTransfer(long id);
        Task<ItemIdMap> ReceiveItemTransfer(long id, long containerId, long slot, ItemIdPropertiesCollection idProps, PropertySet setProperties, PidList removeProperties);
        Task EndItemTransfer(long id);
        Task CancelItemTransfer(long id);

        Task SetPersistent(bool persistent);

        // Test, maintenance
        Task Deactivate();
        Task WritePersistentStorage();
        Task ReadPersistentStorage();
        Task DeletePersistentStorage();
    }
}