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
        public Deletable(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.DeletableAspect;

        public async Task<PropertyValue> DeleteMe()
        {
            var containerId = await this.GetItemId(Pid.Container);
            if (Has.Value(containerId)) {
                var container = await WritableItem(containerId);
                await container.RemoveFromList(Pid.Contains, this.Id);
            }

            var children = await this.GetItemIdList(Pid.Contains);
            foreach (var childId in children) {
                var child = await WritableItem(childId);
                await child.AsDeletable().DeleteMe();
            }

            await this.Delete();
            return true;
        }
    }
}
