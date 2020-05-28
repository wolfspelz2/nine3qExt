using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public class Aspect
    {
        protected ItemStub self;
        protected string Id => self.Id;

        protected async Task<ItemStub> Item(string itemId)
        {
            if (!Has.Value(itemId)) {
                throw new Exception($"{nameof(Aspect)}.{nameof(Item)}: Empty or null itemId");
            }

            var item = (ItemStub)null;
            if (self.ClusterClient != null) {
                item= new ItemStub(self.ClusterClient, itemId, self.Transaction);
            } else if (self.GrainFactory != null) {
                item = new ItemStub(self.GrainFactory, itemId, self.Transaction);
            } else if (self.Simulator != null) {
                item = new ItemStub(self.Simulator, itemId, self.Transaction);
            }
            if (item != null) {
                await self.Transaction.AddItem(item);
                return item;
            } else {
                throw new Exception($"Need valid IClusterClient or IGrainFactory for id={Id}");
            }
        }

        public virtual Pid GetAspectPid() => Pid.FirstAspect;

        protected async Task<bool> IsAspect(Pid pid)
        {
            return await self.Get(pid);
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

        public async Task Run(string action, PropertySet arguments)
        {
            var actions = GetActionList();
            if (actions != null) {
                if (actions.ContainsKey(action)) {
                    await actions[action].Handler(arguments);
                }
            }
        }
    }
}
