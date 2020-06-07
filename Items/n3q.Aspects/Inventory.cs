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

        public enum Action { SetCoordinate }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.SetCoordinate), new ActionDescription() { Handler = async (args) => await SetCoordinate(await Item(args.Get(Pid.InventorySetCoordinateItem)), args.Get(Pid.InventorySetCoordinateX), args.Get(Pid.InventorySetCoordinateY)) } },
            };
        }

        public async Task SetCoordinate(ItemStub item, long x, long y)
        {
            //await AssertAspect();
            if (x >= 0 && y >= 0) {
                await item.ModifyProperties(new PropertySet { [Pid.InventoryX] = x, [Pid.InventoryY] = y }, PidSet.Empty);
            }
        }
    }
}
