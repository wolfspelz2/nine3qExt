using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public class Rezable : Aspect
    {
        public Rezable(Item item) { self = item; }
        public override Pid GetAspectPid() => Pid.RezableAspect;
    }
}
