﻿using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;

namespace n3q.Aspects
{
    public static class CapacityLimitExtensions
    {
        public static CapacityLimit AsCapacityLimit(this Item self) { return new CapacityLimit(self); }
    }

    public class CapacityLimit : Aspect
    {
        public CapacityLimit(Item item) { self = item; }
        public override Pid GetAspectPid() => Pid.ItemCapacityLimitAspect;

        public async Task AssertLimit(Item newItem)
        {
            //await AssertAspect();

            var itemLimit = await self.GetInt(Pid.ContainerItemLimit);

            var itemLimit2 = (long)await self.Get(Pid.ContainerItemLimit);
            long itemLimit3 = await self.Get(Pid.ContainerItemLimit);

            var stacksize = await newItem.GetInt(Pid.Stacksize);
            var currentTotal = 0L;
            foreach (var itemId in await self.GetItemIdSet(Pid.Contains)) {
                currentTotal += await Item(itemId).GetInt(Pid.Stacksize);
            }
            if (currentTotal + stacksize > itemLimit) { throw new SurfaceException(Id, newItem.Id, SurfaceNotification.Fact.NotExecuted, SurfaceNotification.Reason.ItemCapacityLimit); };
        }
    }
}