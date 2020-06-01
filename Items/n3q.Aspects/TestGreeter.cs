using System;
using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public static class TestGreeterExtensions
    {
        public static TestGreeter AsTestGreeter(this ItemStub self) { return new TestGreeter(self); }
    }

    public class TestGreeter : Aspect
    {
        public TestGreeter(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.TestGreeterAspect;

        // No globally visible actions, only called by other aspects

        //public enum Action { Greet }
        //public override ActionList GetActionList()
        //{
        //    return new ActionList() {
        //        { Action.Greet.ToString(), new ActionDescription() { Handler = async (args) => await Greet(args.Get(Pid.TestGreeter_Name)) } },
        //    };
        //}

        public async Task<PropertyValue> Greet(string name)
        {
            var prefix = await this.Get(Pid.TestGreeterPrefix);
            var greeting = prefix + name;
            await this.Set(Pid.TestGreeterResult, greeting);
            return greeting;
        }
    }
}
