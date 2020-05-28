using System;
using System.Threading.Tasks;
using Orleans;
using n3q.Items;
using n3q.GrainInterfaces;
using Orleans.Concurrency;
using n3q.Aspects;

namespace n3q.Grains
{
    [StatelessWorker]
    class WorkerGrain : Grain, IWorker
    {
        #region Interface

        public async Task<PropertyValue> Run(string itemId, Pid aspectPid, string actionName, PropertySet args = null)
        {
            var t = new Transaction();
            var item = new ItemStub(GrainFactory, itemId, t);
            try {
                var aspect = item.AsAspect(aspectPid);
                var result = await aspect.Run(actionName, args);
                t.Commit();
                return result;
            } catch {
                t.Cancel();
            }
            return false;
        }

        #endregion

        #region Internal


        #endregion
    }
}
