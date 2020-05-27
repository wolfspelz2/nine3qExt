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
        Task ModifyProperties(PropertySet modified, PidSet deleted);
        Task<PropertySet> GetProperties(PidSet pids, bool native = false);
        Task AddToSet(Pid pid, PropertyValue value);
        Task DeleteFromSet(Pid pid, PropertyValue value);

        Task<Guid> GetStreamId();
        Task<string> GetStreamNamespace();
        Task Deactivate();
        Task WritePersistentStorage();
        Task ReadPersistentStorage();
        Task DeletePersistentStorage();
    }

}