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
        public Rezable(ItemStub item) { self = item; }
        public override Pid GetAspectPid() => Pid.RezableAspect;

        public enum Action { Rez, Derez }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { Action.Rez.ToString(), new ActionDescription() { Handler = async (args) => await Rez(await Item(args.Get(Pid.RezableRezRoom)), args.Get(Pid.RezableRezX), args.Get(Pid.RezableRezDestination)) } },
                { Action.Derez.ToString(), new ActionDescription() { Handler = async (args) => await Derez(await Item(args.Get(Pid.RezableDerezUser))) } },
            };
        }

        public async Task<PropertyValue> Rez(ItemStub room, long posX, string destination)
        {
            await self.AsRezable().AssertAspect(() => throw new SurfaceException(self.Id, room.Id, SurfaceNotification.Fact.NotRezzed, SurfaceNotification.Reason.ItemIsNotRezable));
            await room.AsContainer().AddChild(self);
            await self.Set(Pid.RezableRezX, posX);
            await self.Set(Pid.RezableIsRezzing, true);
            return true;
        }

        public async Task<PropertyValue> OnRezzed()
        {
            await self.Set(Pid.RezableIsRezzed, true);
            await self.Unset(Pid.RezableIsRezzing);
            return true;
        }

        public async Task<PropertyValue> Derez(ItemStub user)
        {
            await self.AsRezable().AssertAspect(() => throw new SurfaceException(self.Id, user.Id, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemIsNotRezable));
            if (!await self.Get(Pid.RezableIsRezzed)) { throw new SurfaceException(self.Id, user.Id, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemIsNotRezzed); }
            await user.AsContainer().AddChild(self);
            await self.Set(Pid.RezableIsDerezzing, true);
            return true;
        }

        public async Task<PropertyValue> OnDerezzed()
        {
            await self.ModifyProperties(PropertySet.Empty, new PidSet { Pid.RezableRezX, Pid.RezableIsRezzed, Pid.RezableIsRezzing });
            return true;
        }
    }
}
