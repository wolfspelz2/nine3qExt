using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;

namespace n3q.Aspects
{
    public static class CapacityLimitExtensions
    {
        public static CapacityLimit AsItemCapacityLimit(this ItemStub self) { return new CapacityLimit(self); }
    }

    public class CapacityLimit : Aspect
    {
        public CapacityLimit(ItemStub item) { self = item; }
        public override Pid GetAspectPid() => Pid.ItemCapacityLimitAspect;

        public async Task AssertLimit(ItemStub newItem)
        {
            //await AssertAspect();

            var itemLimit = await self.GetInt(Pid.ContainerItemLimit);
            var stacksize = await newItem.GetInt(Pid.Stacksize);
            var currentTotal = 0L;
            foreach (var itemId in (ItemIdList) await self.Get(Pid.Contains)) {
                var child = await Item(itemId);
                currentTotal += await child.GetInt(Pid.Stacksize);
            }
            if (currentTotal + stacksize > itemLimit) { throw new SurfaceException(Id, newItem.Id, SurfaceNotification.Fact.NotExecuted, SurfaceNotification.Reason.ItemCapacityLimit); };
        }
    }
}
