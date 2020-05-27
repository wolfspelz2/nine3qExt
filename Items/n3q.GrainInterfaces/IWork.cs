using System;
using System.Threading.Tasks;
using Orleans;
using n3q.Items;

namespace n3q.GrainInterfaces
{
    public interface IWork : IGrainWithGuidKey
    {
        Task<PropertyValue> Execute(Guid wId, string itemId, Pid aspectPid, string actionName, PropertySet args);
    }

}