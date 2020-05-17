using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using nine3q.Items;

namespace GrainInterfaces
{
    public interface IInventory : IGrainWithStringKey
    {
        Task<ItemId> CreateItem(PropertySet properties);
        Task<bool> DeleteItem(ItemId id);
    }
}