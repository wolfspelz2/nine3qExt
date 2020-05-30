using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class DeletableExtensions
    {
        public static Deletable AsDeletable(this ItemStub self) { return new Deletable(self); }
    }

    public class Deletable : Aspect
    {
        public Deletable(ItemStub item) { self = item; }
        public override Pid GetAspectPid() => Pid.DeletableAspect;

        public async Task<PropertyValue> Delete()
        {
            var containerId = await self.GetItemId(Pid.Container);
            if (Has.Value(containerId)) {
                var container = await Item(containerId);
                await container.RemoveFromList(Pid.Contains, self.Id);
            }

            var children = await self.GetItemIdList(Pid.Contains);
            foreach (var childId in children) {
                var child = await Item(childId);
                await child.AsDeletable().Delete();
            }

            self.MarkForDeletion();
            return true;
        }
    }
}
