using System.Collections.Generic;
using n3q.Items;

namespace n3q.Aspects
{
    public static class AspectRegistry
    {
        public static Dictionary<Pid, Aspect.AspectSpecializer> Aspects = new Dictionary<Pid, Aspect.AspectSpecializer> {
            { Pid.TestGreetedAspect, item => item.AsTestGreeted()},
            { Pid.TestGreeterAspect, item => item.AsTestGreeter()},
            { Pid.ContainerAspect, item => item.AsContainer()},
            { Pid.RezableAspect, item => item.AsRezable()},
            { Pid.ItemCapacityLimitAspect, item => item.AsItemCapacityLimit()},
        };
    }
}
