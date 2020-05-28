using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public static class TestGreetedExtensions
    {
        public static TestGreeted AsTestGreeted(this ItemStub self) { return new TestGreeted(self); }
    }

    public class TestGreeted : Aspect
    {
        public TestGreeted(ItemStub item) { self = item; }
        public override Pid GetAspectPid() => Pid.TestGreetedAspect;

        public enum Action { UseGreeter }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.UseGreeter), new ActionDescription() { Handler = async (args) => await GetGreeting(await Item(args.Get(Pid.TestGreeted_Item)), args.Get(Pid.TestGreeted_Name)) } },
            };
        }

        public async Task GetGreeting(ItemStub passiveItem, string name)
        {
            //await AssertAspect();
            var greeting = await passiveItem.AsTestGreeter().Greet(name);
            await self.Set(Pid.TestGreeted_Result, greeting);
        }

    }
}
