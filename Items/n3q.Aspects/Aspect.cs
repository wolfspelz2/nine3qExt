using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public class Aspect : ItemWriter
    {
        public Aspect(ItemStub item) : base(item.Client, item.Transaction) { }

        public virtual Pid GetAspectPid() => Pid.FirstAspect;

        protected async Task<bool> IsAspect(Pid pid)
        {
            return await Get(pid);
        }

        protected async Task AssertAspect(Pid pid)
        {
            if (!await IsAspect(pid)) {
                throw new Exception($"Item={Id} has no {pid.ToString()}");
            }
        }

        public async Task<bool> IsAspect() { return await IsAspect(GetAspectPid()); }

        public async Task AssertAspect() { await AssertAspect(GetAspectPid()); }

        public async Task AssertAspect(Action action = null)
        {
            if (action != null) {
                try { await AssertAspect(); } catch (Exception) { action.Invoke(); }
            } else {
                await AssertAspect();
            }
        }

        public delegate Aspect AspectSpecializer(ItemStub item);
        public delegate Task ActionHandler(PropertySet args);

        public class ActionDescription
        {
            public ActionHandler Handler { set; get; }
        }

        public class ActionList : Dictionary<string, ActionDescription> { }

        public virtual ActionList GetActionList()
        {
            return null;
        }

        public async Task Execute(string action, PropertySet arguments)
        {
            var actions = GetActionList();
            if (actions != null) {
                if (actions.ContainsKey(action)) {
                    await actions[action].Handler(arguments);
                }
            }
        }

        public static PropertySet MapArgumentsToAspectAction(Dictionary<string, string> args, Aspect aspect, string actionName)
        {
            var pidPrefix = aspect.GetType().Name + actionName;
            var mappedArguments = new PropertySet();
            foreach (var pair in args) {
                var pidName = pidPrefix + pair.Key.Capitalize();
                var pid = pidName.ToEnum(Pid.Unknown);
                if (pid != Pid.Unknown) {
                    mappedArguments[pid] = pair.Value;
                }
            }

            return mappedArguments;
        }

    }
}
