using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using n3q.Tools;
using n3q.Items;
using n3q.GrainInterfaces;
using n3q.Aspects;

namespace n3q.Grains
{
    [StatelessWorker]
    class WorkerGrain : Grain, IWorker
    {
        readonly ILogger _logger;

        public WorkerGrain(ILogger<WorkerGrain> logger)
        {
            _logger = logger;
        }

        #region Interface

        public async Task AspectAction(string itemId, Pid aspectPid, string actionName, PropertySet args = null)
        {
            var transaction = new ItemTransaction();
            var itemClient = new OrleansGrainFactoryItemClient(GrainFactory, itemId);
            var item = new ItemStub(itemClient, transaction);

            try {
                await transaction.Begin(item);
                var aspect = item.AsAspect(aspectPid);
                await aspect.Execute(actionName, args);
                await transaction.Commit();
            } catch (Exception ex) {
                _logger.LogWarning(ex, $"{nameof(AspectAction)} {aspectPid} {actionName} cancel transaction {transaction.Id}");
                await transaction.Cancel();
            }

            // Oder so?
            //await item.WithTransaction(async self => {
            //    var aspect = self.AsAspect(aspectPid);
            //    var result = await aspect.Run(actionName, args);
            //    return result;
            //});
        }

        public async Task<Dictionary<Pid, string>> ItemAction(string userId, string itemId, string actionName, Dictionary<string, string> args)
        {
            var executedActions = new Dictionary<Pid, string>();

            var itemClient = new OrleansGrainFactoryItemClient(GrainFactory, itemId);
            var item = new ItemStub(itemClient);

            await item.WithTransaction(async self => {

                var actionMap = await self.GetMap(Pid.Actions);
                if (!actionMap.TryGetValue(actionName, out var mappedActionName)) {
                    mappedActionName = actionName;
                }

                await self.ForeachAspect(async aspect => {
                    var actions = aspect.GetActionList();
                    if (actions != null) {
                        if (actions.ContainsKey(mappedActionName)) {

                            PropertySet mappedArguments = Aspect.MapArgumentsToAspectAction(args, aspect, mappedActionName);
                            await actions[mappedActionName].Handler(mappedArguments);
                            executedActions.Add(aspect.GetAspectPid(), mappedActionName);

                        }
                    }
                });

            });

            return executedActions;
        }

        #endregion
    }
}
