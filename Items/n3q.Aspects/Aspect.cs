using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans;
using n3q.GrainInterfaces;
using n3q.Items;

namespace n3q.Aspects
{
    public class Aspect
    {
        protected Item self;
        public Item Self => self;
        protected IItem MyGrain => Grain(self);
        protected string Id => self.Id;
        protected IClusterClient Client => self.ClusterClient;

        public IItem Grain(Item item)
        {
            return Client.GetGrain<IItem>(item.Id);
        }

        protected Item Item(string itemId)
        {
            return new Item(Client, itemId);
        }

        public virtual Pid GetAspectPid() => Pid.FirstAspect;

        protected async Task<bool> IsAspect(Pid pid)
        {
            return await MyGrain.GetBool(pid);
        }

        protected async Task AssertAspect(Pid pid)
        {
            if (!await IsAspect(pid)) {
                throw new Exception($"Item={self.Id} has no {pid.ToString()}");
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

        public delegate Aspect AspectSpecializer(Item item);
        public delegate Task<PropertyValue> ActionHandler(PropertySet args);

        public class ActionDescription
        {
            public ActionHandler Handler { set; get; }
        }

        public class ActionList : Dictionary<string, ActionDescription> { }

        public virtual ActionList GetActionList()
        {
            return null;
        }

        public Task<PropertyValue> Run(string action, PropertySet arguments)
        {
            var actions = GetActionList();
            if (actions != null) {
                if (actions.ContainsKey(action)) {
                    return actions[action].Handler(arguments);
                }
            }
            return Task.FromResult(PropertyValue.Empty);
        }
    }
}
