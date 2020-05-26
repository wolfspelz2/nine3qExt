using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public class Container : Aspect
    {
        public Container(Item item) { self = item; }
        public override Pid GetAspectPid() => Pid.ContainerAspect;

        public async Task AddChild(Item child)
        {
            await AssertAspect();
            await Aspect.CapacityLimit(self).AssertLimit(child);
            //await Aspect(self).AsCapacityLimit.AssertLimit(child);
            var currentParent = Item(await child.GetItemId(Pid.Container));
            await currentParent.DeleteFromItemSet(Pid.Contains, Id);
            await self.AddToItemSet(Pid.Contains, child.Id);
            await child.Set(Pid.Container, Id);
        }
    }
}
