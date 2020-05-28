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
                { Action.Rez.ToString(), new ActionDescription() { Handler = async (args) => await Rez(await Item(args.Get(Pid.RezRoom)), args.Get(Pid.RezzedX)) } },
                { Action.Derez.ToString(), new ActionDescription() { Handler = async (args) => await Derez(await Item(args.Get(Pid.DerezUser))) } },
            };
        }

        public async Task<PropertyValue> Rez(ItemStub room, long posX)
        {
            await self.AsRezable().AssertAspect(() => throw new SurfaceException(self.Id, room.Id, SurfaceNotification.Fact.NotRezzed, SurfaceNotification.Reason.ItemIsNotRezable));
            await room.AsContainer().AddChild(self);
            await self.Set(Pid.RezzedX, posX);
            await self.Set(Pid.IsRezzing, true);
            return true;
        }

        public async Task<PropertyValue> OnRezzed()
        {
            await self.Set(Pid.IsRezzed, true);
            await self.Delete(Pid.IsRezzing);
            return true;
        }

        public async Task<PropertyValue> Derez(ItemStub user)
        {
            await self.AsRezable().AssertAspect(() => throw new SurfaceException(self.Id, user.Id, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemIsNotRezable));
            if (!await self.Get(Pid.IsRezzed)) { throw new SurfaceException(self.Id, user.Id, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemIsNotRezzed); }
            await user.AsContainer().AddChild(self);
            await self.Set(Pid.IsDerezzing, true);
            return true;
        }

        public async Task<PropertyValue> OnDerezzed()
        {
            await self.ModifyProperties(PropertySet.Empty, new PidSet { Pid.RezzedX, Pid.IsRezzed, Pid.IsRezzing });
            return true;
        }
    }
}
