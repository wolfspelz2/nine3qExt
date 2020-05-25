using System.Diagnostics.Contracts;

namespace nine3q.Items.Aspects
{
    public static class RezableAspectExtensions
    {
        public static RezableAspect AsRezable(this Item self) { Contract.Requires(self != null); self.AssertAspect(Pid.RezableAspect); return new RezableAspect(self); }
        public static bool IsRezable(this Item self) { Contract.Requires(self != null); return self.IsAspect(Pid.RezableAspect); }
    }

    public class RezableAspect : Aspect
    {
        public RezableAspect(Item self) : base(self) { }

        public enum Action { RezToRoom }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                //{ Action.RezToRoom.ToString(), new ActionDescription() { Handler = (args) => RezToRoom(Inventory.Item(args.GetItem(Pid.Item)), args.GetInt(Pid.Slot)) } },
            };
        }
        public const long NoSlot = 0;

        internal override void OnAspectActivate()
        {
        }

        internal override void OnAspectDelete()
        {
        }

        public void RezToRoom(string room, long x, string destinationUrl)
        {
        }

    }
}
