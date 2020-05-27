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
        Task Set(Pid pid, PropertyValue value);
        Task ModifyProperties(PropertySet modified, PidSet deleted);
        Task<PropertySet> GetProperties(PidSet pids, bool native = false);
        Task AddToItemSet(Pid pid, string itemId);
        Task DeleteFromItemSet(Pid pid, string itemId);

        Task<Guid> GetStreamId();
        Task<string> GetStreamNamespace();
        Task Deactivate();
        Task WritePersistentStorage();
        Task ReadPersistentStorage();
        Task DeletePersistentStorage();
    }

}