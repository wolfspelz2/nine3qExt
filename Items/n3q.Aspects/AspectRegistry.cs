using System.Collections.Generic;
using n3q.Items;

namespace n3q.Aspects
{
    public static class AspectRegistry
    {
        public static Dictionary<Pid, Aspect.AspectSpecializer> Aspects = new Dictionary<Pid, Aspect.AspectSpecializer> {
            { Pid.TestGreetedAspect, item => item.AsTestGreeted()},
            { Pid.TestGreeterAspect, item => item.AsTestGreeter()},
            { Pid.PartnerAspect, item => item.AsPartner()},
            { Pid.ContainerAspect, item => item.AsContainer()},
            { Pid.RezableAspect, item => item.AsRezable()},
            { Pid.MovableAspect, item => item.AsMovable()},
            { Pid.ItemCapacityLimitAspect, item => item.AsItemCapacityLimit()},
            { Pid.DeletableAspect, item => item.AsDeletable()},
            { Pid.InventoryAspect, item => item.AsInventory()},
            { Pid.SettingsAspect, item => item.AsSettings()},
            { Pid.IframeAspect, item => item.AsIframeInteractionItem()},
            { Pid.DocumentAspect, item => item.AsDocument()},
            { Pid.PageClaimAspect, item => item.AsPageClaimer()},
            { Pid.RezableProxyAspect, item => item.AsRezableProxy()},
            { Pid.RoleAspect, item => item.AsRoleAndRights()},
            { Pid.SourceAspect, item => item.AsSource()},
            { Pid.SinkAspect, item => item.AsSink()},
            { Pid.ExtractorAspect, item => item.AsExtractor()},
            { Pid.InjectorAspect, item => item.AsInjector()},
            { Pid.ApplierAspect, item => item.AsApplier()},
        };
    }
}
