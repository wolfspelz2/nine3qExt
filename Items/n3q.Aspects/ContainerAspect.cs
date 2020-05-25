using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public class ContainerAspect : Aspect
    {
        public ContainerAspect(Item item) { self = item; }
        public override Pid GetAspectPid() => Pid.ContainerAspect;

        public async Task AddChild(Item child)
        {
            await AssertAspect();

            var container = GetItem(await Grain(child).GetItem(Pid.Container));
            await Grain(container).DeleteFromItemSet(Pid.Contains, Id);
            await Grain(self).AddToItemSet(Pid.Contains, child.Id);
            await Grain(child).Set(Pid.Container, Id);
        }
    }
}
