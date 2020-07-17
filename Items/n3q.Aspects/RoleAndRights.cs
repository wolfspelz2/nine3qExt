using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class RoleAndRightsExtensions
    {
        public static RoleAndRights AsRole(this ItemStub self) { return new RoleAndRights(self); }
    }

    public class RoleAndRights : Aspect
    {
        public RoleAndRights(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.RoleAspect;
    }
}
