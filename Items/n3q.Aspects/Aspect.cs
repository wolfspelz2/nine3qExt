﻿using System;
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
        protected string Id => self.Id;

        //public IItem Grain(Item item)
        //{
        //    if (self.ClusterClient != null) {
        //        return self.ClusterClient.GetGrain<IItem>(item.Id);
        //    } else if (self.GrainFactory != null) {
        //        return self.GrainFactory.GetGrain<IItem>(item.Id);
        //    } else if (self.Simulator != null) {
        //        return self.Simulator.GetGrain<IItem>(item.Id);
        //    }
        //    throw new Exception($"Need valid IClusterClient or IGrainFactory for id={Id}");
        //}

        protected Item Item(string itemId)
        {
            if (self.ClusterClient != null) {
                return new Item(self.ClusterClient, itemId);
            } else if (self.GrainFactory != null) {
                return new Item(self.GrainFactory, itemId);
            } else if (self.Simulator != null) {
                return new Item(self.Simulator, itemId);
            }
            throw new Exception($"Need valid IClusterClient or IGrainFactory for id={Id}");
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
