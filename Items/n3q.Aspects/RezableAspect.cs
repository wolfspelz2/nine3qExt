using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public class RezableAspect : Aspect
    {
        public RezableAspect(Item item) { self = item; }
        public override Pid GetAspectPid() => Pid.RezableAspect;
    }
}
