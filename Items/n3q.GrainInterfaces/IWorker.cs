using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using n3q.Items;

namespace n3q.GrainInterfaces
{
    public interface IWorker : IGrainWithGuidKey
    {
        Task AspectAction(string itemId, Pid aspectPid, string actionName, PropertySet args = null);
        Task<Dictionary<Pid, string>> ItemAction(string userId, string itemId, string actionName, Dictionary<string, string> args);
    }

}