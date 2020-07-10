using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public static class GreetedExtensions
    {
        public static Greeted AsGreeted(this ItemStub self) { return new Greeted(self); }
    }

    public class Greeted : Aspect
    {
        public Greeted(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.GreetedAspect;

        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(GetGreeting), new ActionDescription() { Handler = async (args) => await GetGreeting(await WritableItem(args.Get(Pid.GreetedGetGreetingGreeter)), args.Get(Pid.GreetedGetGreetingName)) } },
            };
        }

        public async Task GetGreeting(ItemWriter greeter, string name)
        {
            //await AssertAspect();
            var greeting = await greeter.AsGreeter().Greet(name);
            await this.Set(Pid.GreetedResult, greeting);
        }

    }
}
