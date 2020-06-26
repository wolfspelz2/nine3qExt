using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class IframeInteractionItemExtensions
    {
        public static IframeInteractionItem AsIframeInteractionItem(this ItemStub self) { return new IframeInteractionItem(self); }
    }

    public class IframeInteractionItem : Aspect
    {
        public IframeInteractionItem(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.IframeAspect;
    }
}
