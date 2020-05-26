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

            {
                var container = GetItem(await child.Grain.GetItem(Pid.Container));
                await container.Grain.DeleteFromItemSet(Pid.Contains, Id);
                await self.Grain.AddToItemSet(Pid.Contains, child.Id);
                await child.Grain.Set(Pid.Container, Id);
            }

            {
                var container = GetItem(await child.I.GetItem(Pid.Container));
                await container.I.DeleteFromItemSet(Pid.Contains, Id);
                await self.I.AddToItemSet(Pid.Contains, child.Id);
                await child.I.Set(Pid.Container, Id);
            }
        }
    }
}
