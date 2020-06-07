using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class RezzableProxyExtensions
    {
        public static RezzableProxy AsRezzableProxy(this ItemStub self) { return new RezzableProxy(self); }
    }

    public class RezzableProxy : Aspect
    {
        public RezzableProxy(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.RezzableProxyAspect;
    }
}
