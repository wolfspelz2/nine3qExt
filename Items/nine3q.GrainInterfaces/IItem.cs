using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using nine3q.Items;
using nine3q.Tools;

namespace nine3q.GrainInterfaces
{
    public interface IItem : IGrainWithStringKey
    {
        Task<Guid> GetStreamId();
        Task<string> GetStreamNamespace();
        Task Deactivate();
        Task WritePersistentStorage();
        Task ReadPersistentStorage();
        Task DeletePersistentStorage();
    }

}