using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;

namespace n3q.Aspects
{
    public static class MovableExtensions
    {
        public static Movable AsMovable(this ItemStub self) { return new Movable(self); }
    }

    public class Movable : Aspect
    {
        public Movable(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.MovableAspect;

        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(MoveTo), new ActionDescription() { Handler = async (args) => await MoveTo(args.Get(Pid.MovableMoveToX)) } },
            };
        }

        public async Task<PropertyValue> MoveTo(long x)
        {
            await this.AsMovable().AssertAspect(() => throw new ItemException(this.Id, this.Id, ItemNotification.Fact.NotMoved, ItemNotification.Reason.ItemIsNotMovable));
            await this.Set(Pid.RezzedX, x);
            return true;
        }
    }
}
