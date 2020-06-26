using n3q.Items;

namespace n3q.Aspects
{
    public static class RezableProxyExtensions
    {
        public static RezableProxy AsRezableProxy(this ItemStub self) { return new RezableProxy(self); }
    }

    public class RezableProxy : Aspect
    {
        public RezableProxy(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.RezableProxyAspect;
    }
}
