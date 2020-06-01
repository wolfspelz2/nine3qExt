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
        Task AddToListProperty(Pid pid, PropertyValue value, Guid tid);
        Task RemoveFromListProperty(Pid pid, PropertyValue value, Guid tid);

        Task<PropertySet> GetPropertiesX(PidSet pids, bool native = false);

        Task Delete(Guid tid);

        Task BeginTransaction(Guid tid);
        Task EndTransaction(Guid tid, bool success);

        Task Deactivate();
        Task WritePersistentStorage();
        Task ReadPersistentStorage();
        Task DeletePersistentStorage();
    }

}