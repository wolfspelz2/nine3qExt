using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;

namespace n3q.Aspects
{
    public static class InventoryExtensions
    {
        public static Inventory AsInventory(this ItemStub self) { return new Inventory(self); }
    }

    public class Inventory : Aspect
    {
        public Inventory(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.InventoryAspect;

        public enum Action { SetCoordinates, SetItemCoordinates }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.SetCoordinates), new ActionDescription() { Handler = async (args) => await SetCoordinates(args.Get(Pid.InventorySetCoordinatesLeft), args.Get(Pid.InventorySetCoordinatesBottom), args.Get(Pid.InventorySetCoordinatesWidth), args.Get(Pid.InventorySetCoordinatesHeight)) } },
                { nameof(Action.SetItemCoordinates), new ActionDescription() { Handler = async (args) => await SetItemCoordinates(await Item(args.Get(Pid.InventorySetItemCoordinatesItem)), args.Get(Pid.InventorySetItemCoordinatesX), args.Get(Pid.InventorySetItemCoordinatesY)) } },
            };
        }

        public async Task SetCoordinates(long left, long bottom, long width, long height)
        {
            //await AssertAspect();
            if (left >= 0 && bottom >= 0 && width >= 0 && height >= 0) {
                await this.ModifyProperties(new PropertySet { [Pid.Left] = left, [Pid.Bottom] = bottom, [Pid.Width] = left, [Pid.Height] = bottom }, PidSet.Empty);
            }
        }

        public async Task SetItemCoordinates(ItemStub item, long x, long y)
        {
            //await AssertAspect();
            if (x >= 0 && y >= 0) {
                await item.ModifyProperties(new PropertySet { [Pid.InventoryX] = x, [Pid.InventoryY] = y }, PidSet.Empty);
            }
        }
    }
}
