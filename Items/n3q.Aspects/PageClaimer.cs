using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class PageClaimerExtensions
    {
        public static PageClaimer AsPageClaimer(this ItemStub self) { return new PageClaimer(self); }
    }

    public class PageClaimer : Aspect
    {
        public PageClaimer(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.PageClaimAspect;
    }
}
