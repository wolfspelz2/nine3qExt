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

        Task<PropertySet> GetItemProperties(long id, PidList pids, bool native = false);

        Task<long> GetItemByName(string name);

        // Test, maintenance
        Task Deactivate();
        Task WritePersistentStorage();
        Task ReadPersistentStorage();
        Task DeletePersistentStorage();
    }
}