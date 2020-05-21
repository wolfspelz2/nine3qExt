using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using nine3q.Items;

namespace nine3q.GrainInterfaces
{
    public interface IInventory : IGrainWithStringKey
    {
        // Items
        Task<long> CreateItem(PropertySet properties);
        Task<bool> DeleteItem(long id);
        Task<long> GetItemByName(string name);
        Task<ItemIdSet> GetItemIds();

        // Properties
        Task SetItemProperties(long id, PropertySet properties);
        Task<PropertySet> GetItemProperties(long id, PidList pids, bool native = false);
        Task<int> DeleteItemProperties(long id, PidList pids);
        Task ModifyItemProperties(long id, PropertySet modified, PidList deleted);
        Task<ItemIdPropertiesCollection> GetItemIdsAndValuesByProperty(Pid filterProperty, PidList desiredProperties);

        // Container
        Task AddChildToContainer(long id, long containerId, long slot);
        Task RemoveChildFromContainer(long id, long containerId);

        // Transfer
        Task<ItemIdPropertiesCollection> BeginItemTransfer(long id);
        Task<ItemIdMap> ReceiveItemTransfer(long id, long containerId, long slot, ItemIdPropertiesCollection idProps, PropertySet finallySetProperties, PidList finallyDeleteProperties);
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