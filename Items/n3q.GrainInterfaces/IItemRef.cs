using System;
using System.Threading.Tasks;
using Orleans;

namespace n3q.GrainInterfaces
{
    public interface IItemRef : IGrainWithStringKey
    {
        Task SetItem(string itemId);
        Task<string> GetItem();
        Task Delete();

        Task DeletePersistentStorage();
        Task ReloadPersistentStorage();
        Task Deactivate();
    }
}