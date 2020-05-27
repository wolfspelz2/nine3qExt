﻿using System;
using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public static class ContainerExtensions
    {
        public static Container AsContainer(this Item self) { return new Container(self); }
    }

    public class Container : Aspect
    {
        public Container(Item item) { self = item; }
        public override Pid GetAspectPid() => Pid.ContainerAspect;

        public async Task AddChild(Item child)
        {
            await AssertAspect();
            await self.AsCapacityLimit().AssertLimit(child);

            var currentParent = Item(await child.GetItemId(Pid.Container));
            await currentParent.DeleteFromItemSet(Pid.Contains, child.Id);
            await self.AddToItemSet(Pid.Contains, child.Id);
            await child.Set(Pid.Container, Id);
        }
    }
}