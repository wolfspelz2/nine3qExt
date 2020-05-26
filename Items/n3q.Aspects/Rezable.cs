using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public static class RezableExtension
    {
        public static Rezable AsRezable(this Item self) => new Rezable(self);
    }

    public class Rezable : Aspect
    {
        public Rezable(Item item) { self = item; }
        public override Pid GetAspectPid() => Pid.RezableAspect;
    }
}
