using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class RezableExtensions
    {
        public static Rezable AsRezable(this ItemStub self) { return new Rezable(self); }
    }

    public class Rezable : Aspect
    {
        public Rezable(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.RezableAspect;

        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Rez), new ActionDescription() { Handler = async (args) => await Rez(await WritableItem(args.Get(Pid.RezableRezTo)), args.Get(Pid.RezableRezX), args.Get(Pid.RezableRezDestination)) } },
                { nameof(OnRezzed), new ActionDescription() { Handler = async (args) => await OnRezzed() } },
                { nameof(OnRezFailed), new ActionDescription() { Handler = async (args) => await OnRezFailed() } },
                { nameof(Derez), new ActionDescription() { Handler = async (args) => await Derez(await WritableItem(args.Get(Pid.RezableDerezTo)), args.Get(Pid.RezableDerezX), args.Get(Pid.RezableDerezY)) } },
                { nameof(OnDerezzed), new ActionDescription() { Handler = async (args) => await OnDerezzed() } },
            };
        }

        public async Task<PropertyValue> Rez(ItemWriter toRoom, long x, string destination)
        {
            await this.AsRezable().AssertAspect(() => throw new ItemException(this.Id, toRoom.Id, ItemNotification.Fact.NotRezzed, ItemNotification.Reason.ItemIsNotRezable));

            var props = await this.Get(new PidSet { Pid.InventoryX, Pid.InventoryY, Pid.Container, Pid.Owner });
            var parentId = props.GetItemId(Pid.Container);
            var inventoryX = props.GetInt(Pid.InventoryX);
            var inventoryY = props.GetInt(Pid.InventoryY);
            var owner = props.GetString(Pid.Owner);
            var label = props.GetString(Pid.Label);

            await toRoom.AsContainer().AddChild(this);
            await this.Modify(new PropertySet { [Pid.RezzedX] = x, [Pid.RezableIsRezzing] = true, [Pid.RezableOrigin] = parentId, [Pid.RezzedDestination] = destination }, PidSet.Empty);

            if (Has.Value(parentId)) {
                var proxy = await NewItemFromTemplate("PageProxy", owner); // DevSpec.Template.PageProxy
                await proxy.Modify(new PropertySet { [Pid.RezableProxyTargetItem] = this.Id, [Pid.RezableProxyDestination] = destination, [Pid.RezableProxyTargetLabel] = label, [Pid.InventoryX] = inventoryX, [Pid.InventoryY] = inventoryY }, PidSet.Empty);

                var parent = await WritableItem(parentId);
                await parent.AsContainer().AddChild(proxy);
            }

            return true;
        }

        public async Task<PropertyValue> OnRezzed()
        {
            await this.Set(Pid.RezableIsRezzed, true);
            await this.Unset(Pid.RezableIsRezzing);
            return true;
        }

        public async Task<PropertyValue> OnRezFailed()
        {
            var originId = await this.GetItemId(Pid.RezableOrigin);
            await this.Modify(PropertySet.Empty, new PidSet { Pid.RezableIsRezzing, Pid.RezableOrigin, Pid.RezzedX });
            if (Has.Value(originId)) {
                var origin = await WritableItem(originId);
                await origin.AsContainer().AddChild(this);
            }
            return true;
        }

        public async Task<PropertyValue> Derez(ItemWriter toUser, long x, long y)
        {
            await this.AsRezable().AssertAspect(() => throw new ItemException(this.Id, toUser.Id, ItemNotification.Fact.NotDerezzed, ItemNotification.Reason.ItemIsNotRezable));
            if (!await this.Get(Pid.RezableIsRezzed)) { throw new ItemException(this.Id, toUser.Id, ItemNotification.Fact.NotDerezzed, ItemNotification.Reason.ItemIsNotRezzed); }

            await toUser.AsContainer().AddChild(this);

            if (x >= 0 && y >= 0) {
                await toUser.AsInventory().SetItemCoordinates(this, x, y);
            }
            await this.Set(Pid.RezableIsDerezzing, true);

            return true;
        }

        public async Task<PropertyValue> OnDerezzed()
        {
            await this.Modify(PropertySet.Empty, new PidSet { Pid.RezableRezX, Pid.RezableIsRezzed, Pid.RezableIsRezzing });
            return true;
        }
    }
}
