using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public class ContainerAspect : ItemAspect
    {
        public ContainerAspect(Item item) { self = item; }

        private async Task AssertAspect() { await AssertAspect(Pid.ContainerAspect); }

        public async Task AddItem(Item child)
        {
            await AssertAspect();

            await Grain(self).AddToItemSet(Pid.Contains, child.Id);
            await Grain(child).Set(Pid.Container, Id);
        }

    }
}
