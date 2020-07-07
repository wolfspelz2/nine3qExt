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

        public enum Action { Rez, OnRezzed, OnRezFailed, Derez, OnDerezzed }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.Rez), new ActionDescription() { Handler = async (args) => await Rez(await Item(args.Get(Pid.RezableRezTo)), args.Get(Pid.RezableRezX), args.Get(Pid.RezableRezDestination)) } },
                { nameof(Action.OnRezzed), new ActionDescription() { Handler = async (args) => await OnRezzed() } },
                { nameof(Action.OnRezFailed), new ActionDescription() { Handler = async (args) => await OnRezFailed() } },
                { nameof(Action.Derez), new ActionDescription() { Handler = async (args) => await Derez(await Item(args.Get(Pid.RezableDerezTo)), args.Get(Pid.RezableDerezX), args.Get(Pid.RezableDerezY)) } },
                { nameof(Action.OnDerezzed), new ActionDescription() { Handler = async (args) => await OnDerezzed() } },
            };
        }

        public async Task<PropertyValue> Rez(ItemStub toRoom, long x, string destination)
        {
            await this.AsRezable().AssertAspect(() => throw new SurfaceException(this.Id, toRoom.Id, SurfaceNotification.Fact.NotRezzed, SurfaceNotification.Reason.ItemIsNotRezable));
            var parentId = await this.GetItemId(Pid.Container);
            await toRoom.AsContainer().AddChild(this);
            await this.Modify(new PropertySet { [Pid.RezzedX] = x, [Pid.RezableIsRezzing] = true, [Pid.RezableOrigin] = parentId }, PidSet.Empty);
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
                var origin = await Item(originId);
                await origin.AsContainer().AddChild(this);
            }
            return true;
        }

        public async Task<PropertyValue> Derez(ItemStub toUser, long x, long y)
        {
            await this.AsRezable().AssertAspect(() => throw new SurfaceException(this.Id, toUser.Id, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemIsNotRezable));
            if (!await this.Get(Pid.RezableIsRezzed)) { throw new SurfaceException(this.Id, toUser.Id, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemIsNotRezzed); }

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
