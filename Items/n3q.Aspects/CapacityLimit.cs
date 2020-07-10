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
        public CapacityLimit(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.ItemCapacityLimitAspect;

        public async Task AssertLimit(ItemWriter newItem)
        {
            //await AssertAspect();

            var itemLimit = await this.GetInt(Pid.ContainerItemLimit);
            if (itemLimit > 0) {
                var stacksize = await newItem.GetInt(Pid.Stacksize);
                var currentTotal = 0L;
                foreach (var itemId in (ValueList)await this.Get(Pid.Contains)) {
                    var child = ReadonlyItem(itemId);
                    currentTotal += await child.GetInt(Pid.Stacksize);
                }
                if (currentTotal + stacksize > itemLimit) { throw new ItemException(Id, newItem.Id, ItemNotification.Fact.NotExecuted, ItemNotification.Reason.ItemCapacityLimit); };
            }
        }
    }
}
