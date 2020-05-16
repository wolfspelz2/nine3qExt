using System.Collections.Generic;
//using nine3q.Items.Aspects;

namespace nine3q.Items
{
    public static class AspectRegistry
    {
        public static Dictionary<Pid, Aspect.AspectSpecializer> Aspects = new Dictionary<Pid, Aspect.AspectSpecializer> {
            { Pid.IsTest1, item => item.AsTest1()},
            { Pid.IsTest2, item => item.AsTest2()},
            //{ Pid.IsContainer, item => item.AsContainer()},
            //{ Pid.IsTrashCan, item => item.AsTrash()},
            //{ Pid.IsSource, item => item.AsSource()},
            //{ Pid.IsSink, item => item.AsSink()},
            //{ Pid.IsExtractor, item => item.AsExtractor()},
            //{ Pid.IsInjector, item => item.AsInjector()},
            //{ Pid.IsDeletee, item => item.AsDeletee()},
            //{ Pid.IsApplier, item => item.AsApplier()},
            //{ Pid.IsCondition, item => item.AsCondition()},
        };
    }
}
