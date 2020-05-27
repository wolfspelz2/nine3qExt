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
        Task Set(Pid pid, string value);
        Task Set(Pid pid, long value);
        Task Set(Pid pid, double value);
        Task Set(Pid pid, bool value);
        Task Set(Pid pid, ItemIdSet value);
        Task AddToItemSet(Pid pid, string itemId);
        Task DeleteFromItemSet(Pid pid, string itemId);

        Task Delete(Pid pid);
        Task Modify(PropertySet modified, PidSet deleted);

        Task<PropertyValue> Get(Pid pid);
        Task<string> GetString(Pid pid);
        Task<long> GetInt(Pid pid);
        Task<double> GetFloat(Pid pid);
        Task<bool> GetBool(Pid pid);
        Task<string> GetItemId(Pid pid);
        Task<ItemIdSet> GetItemIdSet(Pid pid);
        Task<PropertySet> GetProperties(PidSet pids, bool native = false);

        //Task<Guid> BeginTransaction();
        //Task CommitTransaction(Guid t);
        //Task CancelTransaction(Guid t);

        Task<Guid> GetStreamId();
        Task<string> GetStreamNamespace();
        Task Deactivate();
        Task WritePersistentStorage();
        Task ReadPersistentStorage();
        Task DeletePersistentStorage();
    }

}