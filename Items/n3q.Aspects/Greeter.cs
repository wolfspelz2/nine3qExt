using System;
using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public static class GreeterExtensions
    {
        public static Greeter AsGreeter(this ItemStub self) { return new Greeter(self); }
    }

    public class Greeter : Aspect
    {
        public Greeter(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.GreeterAspect;

        public async Task<PropertyValue> Greet(string name)
        {
            var prefix = await this.Get(Pid.GreeterPrefix);
            var greeting = prefix + name;
            await this.Set(Pid.GreeterResult, greeting);
            return greeting;
        }
    }
}
