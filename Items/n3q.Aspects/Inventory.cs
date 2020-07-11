using System.Threading.Tasks;
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

        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Initialize), new ActionDescription() { Handler = async (args) => await Initialize() } },
                { nameof(SetItemCoordinates), new ActionDescription() { Handler = async (args) => await SetItemCoordinates(await WritableItem(args.Get(Pid.InventorySetItemCoordinatesItem)), args.Get(Pid.InventorySetItemCoordinatesX), args.Get(Pid.InventorySetItemCoordinatesY)) } },
            };
        }

        public async Task Initialize()
        {
            var flag = await NewItemFromTemplate("PirateFlag");
            await this.AsContainer().AddChild(flag);
        }

        public async Task SetItemCoordinates(ItemWriter item, long x, long y)
        {
            //await AssertAspect();
            if (x >= 0 && y >= 0) {
                await item.Modify(new PropertySet { [Pid.InventoryX] = x, [Pid.InventoryY] = y }, PidSet.Empty);
            }
        }
    }
}
