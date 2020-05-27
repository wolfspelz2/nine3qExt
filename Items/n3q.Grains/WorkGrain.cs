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
    class WorkGrain : Grain, IWork
    {
        Item GetItem(string id) { return new Item(GrainFactory, id); }

        #region Interface

        public Task Execute(Guid wId, string itemId, Pid aspectPid, string actionName, PropertySet args)
        {
            var item = GetItem(itemId);
            var aspect = item.AsAspect(aspectPid);

            return Task.CompletedTask;
        }

        #endregion

        #region Internal


        #endregion
    }
}
