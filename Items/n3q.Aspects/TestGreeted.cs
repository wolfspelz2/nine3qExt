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
        public TestGreeted(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.TestGreetedAspect;

        public enum Action { GetGreeting }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.GetGreeting), new ActionDescription() { Handler = async (args) => await GetGreeting(await WritableItem(args.Get(Pid.TestGreetedGetGreetingGreeter)), args.Get(Pid.TestGreetedGetGreetingName)) } },
            };
        }

        public async Task GetGreeting(ItemWriter greeter, string name)
        {
            //await AssertAspect();
            var greeting = await greeter.AsTestGreeter().Greet(name);
            await this.Set(Pid.TestGreetedResult, greeting);
        }

    }
}
