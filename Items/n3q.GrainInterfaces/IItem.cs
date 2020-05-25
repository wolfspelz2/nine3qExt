using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using n3q.Items;
using n3q.Tools;

namespace n3q.GrainInterfaces
{
    public interface IItem : IGrainWithStringKey
    {
        Task<Guid> GetStreamId();
        Task<string> GetStreamNamespace();
        Task Deactivate();
        Task WritePersistentStorage();
        Task ReadPersistentStorage();
        Task DeletePersistentStorage();

        Task<bool> GetBool(Pid pid);
        //Task Transfer(string destContainer);
        Task AddToItemSet(Pid pid, string itemId);
        Task Set(Pid pid, string value);
    }

}