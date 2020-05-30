using System;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        #region Interface

        public async Task AspectAction(string itemId, Pid aspectPid, string actionName, PropertySet args = null)
        {
            var t = new ItemTransaction();
            var item = new ItemStub(GrainFactory, itemId, t);

            try {
                await t.Begin(item);
                var aspect = item.AsAspect(aspectPid);
                await aspect.Execute(actionName, args);
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

        public async Task<Dictionary<Pid, string>> ItemAction(string userId, string itemId, string actionName, Dictionary<string, string> args)
        {
            var executedActions = new Dictionary<Pid, string>();

            var item = new ItemStub(GrainFactory, itemId);
            await item.WithTransaction(async self => {

                var actionMap = await self.GetMap(Pid.Actions);
                if (!actionMap.TryGetValue(actionName, out var mappedActionName)) {
                    mappedActionName = actionName;
                }

                await self.ForeachAspect(async aspect => {
                    var actions = aspect.GetActionList();
                    if (actions != null) {
                        if (actions.ContainsKey(mappedActionName)) {

                            PropertySet mappedArguments = MapArgumentsToAspectAction(args, aspect, mappedActionName);
                            await actions[mappedActionName].Handler(mappedArguments);
                            executedActions.Add(aspect.GetAspectPid(), mappedActionName);

                        }
                    }
                });

            });

            return executedActions;
        }

        #endregion

        #region Internal

        private static PropertySet MapArgumentsToAspectAction(Dictionary<string, string> args, Aspect aspect, string actionName)
        {
            var pidPrefix = aspect.GetType().Name + actionName;
            var mappedArguments = new PropertySet();
            foreach (var pair in args) {
                var pidName = pidPrefix + Capitalize(pair.Key);
                var pid = pidName.ToEnum(Pid.Unknown);
                if (pid != Pid.Unknown) {
                    mappedArguments[pid] = pair.Value;
                }
            }

            return mappedArguments;
        }

        private static string Capitalize(string s)
        {
            return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
        }

        #endregion
    }
}
