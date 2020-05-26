using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public static class RezableExtensions
    {
        public static Rezable AsRezable(this Item self) { return new Rezable(self); }
    }

    public class Rezable : Aspect
    {
        public Rezable(Item item) { self = item; }
        public override Pid GetAspectPid() => Pid.RezableAspect;
    }
}
