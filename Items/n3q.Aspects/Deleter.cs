using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class DeleterExtensions
    {
        public static Deleter AsDeleter(this ItemStub self) { return new Deleter(self); }
    }

    public class Deleter : Aspect
    {
        public Deleter(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.DeleterAspect;

        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Delete), new ActionDescription() { Handler = async (args) => await Delete(await WritableItem(args.Get(Pid.DeleterDeleteVictim))) } },
                { nameof(DeleteChildren), new ActionDescription() { Handler = async (args) => await DeleteChildren() } },
            };
        }

        public async Task Delete(ItemWriter victim)
        {
            await AssertAspect();
            await victim.AsDeletable().DeleteMe();
        }

        public async Task DeleteChildren()
        {
            await AssertAspect();
            var children = await this.GetItemIdList(Pid.Contains);
            foreach (var childId in children) {
                var child = await WritableItem(childId);
                await Delete(child);
            }
        }
    }
}
