﻿using System;
using System.Threading.Tasks;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class ContainerExtensions
    {
        public static Container AsContainer(this ItemStub self) { return new Container(self); }
    }

    public class Container : Aspect
    {
        public Container(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.ContainerAspect;

        public async Task AddChild(ItemStub child)
        {
            //var props = await child.Grain.GetProperties(new PidSet { Pid.Container }, false);
            //child.Simulator = new ItemSiloSimulator();
            var props = await child.GetProperties(new PidSet { Pid.TestInt }, true);

            await AssertAspect();
            await this.AsItemCapacityLimit().AssertLimit(child);

            var parentId = await child.GetItemId(Pid.Container);
            if (Has.Value(parentId)) {
                var currentParent = await Item(parentId);
                await currentParent.RemoveFromList(Pid.Contains, child.Id);
            }
            await this.AddToList(Pid.Contains, child.Id);
            await child.Set(Pid.Container, Id);
        }

        public async Task RemoveChild(ItemStub child)
        {
            await AssertAspect();

            await this.RemoveFromList(Pid.Contains, child.Id);
            await child.Unset(Pid.Container);
        }
    }
}
