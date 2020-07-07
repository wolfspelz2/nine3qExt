using System;
using System.Threading.Tasks;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class PartnerExtensions
    {
        public static Partner AsPartner(this ItemStub self) { return new Partner(self); }
    }

    public class Partner : Aspect
    {
        public Partner(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.PartnerAspect;
    }
}
