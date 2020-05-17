using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using nine3q.Items;

namespace nine3q.GrainInterfaces
{
    public interface IInventory : IGrainWithStringKey
    {
        Task<ItemId> CreateItem(PropertySet properties);
        Task<bool> DeleteItem(ItemId id);

        Task<PropertySet> GetItemProperties(ItemId id, PidList pids, bool native = false);

        Task<ItemId> GetItemByName(string name);

        // Test, maintenance
        Task DeletePermanentStorage();
    }
}