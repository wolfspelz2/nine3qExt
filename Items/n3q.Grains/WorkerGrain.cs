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

        public async Task Run(string itemId, Pid aspectPid, string actionName, PropertySet args = null)
        {
            var t = new ItemTransaction();
            var item = new ItemStub(GrainFactory, itemId, t);

            try {
                await t.Begin(item);
                var aspect = item.AsAspect(aspectPid);
                await aspect.Run(actionName, args);
                await t.Commit();
            } catch (Exception ex) {
                _ = ex;
                await t.Cancel();
            }

            // Oder so?
            //await item.WithTransaction(async () => {
            //    var aspect = item.AsAspect(aspectPid);
            //    var result = await aspect.Run(actionName, args);
            //    t.Commit();
            //    return result;
            //});
        }

        #endregion

        #region Internal


        #endregion
    }
}
