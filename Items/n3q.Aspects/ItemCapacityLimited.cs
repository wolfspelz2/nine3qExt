using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;

namespace n3q.Aspects
{
    public static class ItemCapacityLimitedExtensions
    {
        public static ItemCapacityLimited AsItemCapacityLimited(this ItemStub self) { return new ItemCapacityLimited(self); }
    }

    public class ItemCapacityLimited : Aspect
    {
        public ItemCapacityLimited(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.ItemCapacityLimitedAspect;

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
