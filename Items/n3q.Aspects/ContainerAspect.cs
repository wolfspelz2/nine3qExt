using System;
using System.Threading.Tasks;
using n3q.Items;
using Orleans;

namespace n3q.Aspects
{
    public class ContainerAspect : Aspect
    {
        public ContainerAspect(Item item) { self = item; }

        private async Task AssertAspect() { await AssertAspect(Pid.ContainerAspect); }

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
