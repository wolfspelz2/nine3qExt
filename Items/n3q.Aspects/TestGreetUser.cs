using System;
using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public static class TestGreetUserExtensions
    {
        public static TestGreetUser AsTestGreetUser(this Item self) { return new TestGreetUser(self); }
    }

    public class TestGreetUser : Aspect
    {
        public TestGreetUser(Item item) { self = item; }
        public override Pid GetAspectPid() => Pid.TestGreetUserAspect;

        public enum Action { UseGreeter }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.UseGreeter), new ActionDescription() { Handler = async (args) => await UseGreeter(Item(args.Get(Pid.Item)), args.Get(Pid.Name)) } },
            };
        }

        public async Task<PropertyValue> UseGreeter(Item passiveItem, string name)
        {
            //await AssertAspect();
            var greeting = await passiveItem.AsTestGreeter().Greet(name);
            return greeting;
            //return PropertyValue.Empty;
        }

    }
}
