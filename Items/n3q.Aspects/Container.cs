using System;
using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public static class ContainerExtensions
    {
        public static Container AsContainer(this ItemStub self) { return new Container(self); }
    }

    public class Container : Aspect
    {
        public Container(ItemStub item) { self = item; }
        public override Pid GetAspectPid() => Pid.ContainerAspect;

        public async Task AddChild(ItemStub child)
        {
            await AssertAspect();
            await self.AsItemCapacityLimit().AssertLimit(child);

            var currentParent = await Item(await child.GetItemId(Pid.Container));
            await currentParent.DeleteFromList(Pid.Contains, child.Id);
            await self.AddToList(Pid.Contains, child.Id);
            await child.Set(Pid.Container, Id);
        }
    }
}
