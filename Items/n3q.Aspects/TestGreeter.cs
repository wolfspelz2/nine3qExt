using System;
using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public static class TestGreeterExtensions
    {
        public static TestGreeter AsTestGreeter(this Item self) { return new TestGreeter(self); }
    }

    public class TestGreeter : Aspect
    {
        public TestGreeter(Item item) { self = item; }
        public override Pid GetAspectPid() => Pid.TestGreeterAspect;

        public enum Action { Greet }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { Action.Greet.ToString(), new ActionDescription() { Handler = async (args) => await Greet(args.Get(Pid.Name)) } },
            };
        }

        public async Task<PropertyValue> Greet(string name)
        {
            await Task.CompletedTask;
            var prefix = await self.Get(Pid.TestGreeterPrefix);
            var greeting = prefix + name;
            return greeting;
        }
    }
}
