using System;
using System.Threading.Tasks;
using Orleans;
using n3q.Items;

namespace n3q.GrainInterfaces
{
    public interface IWorker : IGrainWithGuidKey
    {
        Task Run(string itemId, Pid aspectPid, string actionName, PropertySet args = null);
    }

}