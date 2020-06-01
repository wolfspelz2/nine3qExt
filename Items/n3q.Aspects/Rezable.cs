using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;

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

        public enum Action { Rez, OnRezzed, Derez, OnDerezzed }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.Rez), new ActionDescription() { Handler = async (args) => await Rez(await Item(args.Get(Pid.RezableRezTo)), args.Get(Pid.RezableRezX), args.Get(Pid.RezableRezDestination)) } },
                { nameof(Action.OnRezzed), new ActionDescription() { Handler = async (args) => await OnRezzed() } },
                { nameof(Action.Derez), new ActionDescription() { Handler = async (args) => await Derez(await Item(args.Get(Pid.RezableDerezTo))) } },
                { nameof(Action.OnDerezzed), new ActionDescription() { Handler = async (args) => await OnDerezzed() } },
            };
        }

        public async Task<PropertyValue> Rez(ItemStub room, long posX, string destination)
        {
            await this.AsRezable().AssertAspect(() => throw new SurfaceException(this.Id, room.Id, SurfaceNotification.Fact.NotRezzed, SurfaceNotification.Reason.ItemIsNotRezable));
            await room.AsContainer().AddChild(this);
            await this.Set(Pid.RezzedX, posX);
            await this.Set(Pid.RezableIsRezzing, true);
            return true;
        }

        public async Task<PropertyValue> OnRezzed()
        {
            await this.Set(Pid.RezableIsRezzed, true);
            await this.Unset(Pid.RezableIsRezzing);
            return true;
        }

        public async Task<PropertyValue> Derez(ItemStub user)
        {
            await this.AsRezable().AssertAspect(() => throw new SurfaceException(this.Id, user.Id, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemIsNotRezable));
            if (!await this.Get(Pid.RezableIsRezzed)) { throw new SurfaceException(this.Id, user.Id, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemIsNotRezzed); }
            await user.AsContainer().AddChild(this);
            await this.Set(Pid.RezableIsDerezzing, true);
            return true;
        }

        public async Task<PropertyValue> OnDerezzed()
        {
            await this.ModifyProperties(PropertySet.Empty, new PidSet { Pid.RezableRezX, Pid.RezableIsRezzed, Pid.RezableIsRezzing });
            return true;
        }
    }
}
