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
        Task ModifyProperties(PropertySet modified, PidSet deleted, Guid tid);
        Task AddToList(Pid pid, PropertyValue value, Guid tid);
        Task DeleteFromList(Pid pid, PropertyValue value, Guid tid);
        Task<PropertySet> GetProperties(PidSet pids, bool native = false);

        Task BeginTransaction(Guid tid);
        Task EndTransaction(Guid tid, bool success);

        Task<Guid> GetStreamId();
        Task<string> GetStreamNamespace();
        Task Deactivate();
        Task WritePersistentStorage();
        Task ReadPersistentStorage();
        Task DeletePersistentStorage();
    }

}