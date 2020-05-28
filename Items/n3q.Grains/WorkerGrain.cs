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
            var item = new Item(GrainFactory, itemId, Guid.NewGuid());
            var aspect = item.AsAspect(aspectPid);
            return await aspect.Run(actionName, args);
        }

        #endregion

        #region Internal


        #endregion
    }
}
