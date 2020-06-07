﻿using System.Collections.Generic;
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
            { Pid.DeletableAspect, item => item.AsDeletable()},
            { Pid.InventoryAspect, item => item.AsInventory()},
            { Pid.IframeAspect, item => item.AsIframeInteractionItem()},
            { Pid.PageClaimAspect, item => item.AsPageClaimer()},
            { Pid.RezzableProxyAspect, item => item.AsRezzableProxy()},
            { Pid.RoleAspect, item => item.AsRoleAndRights()},
            { Pid.SourceAspect, item => item.AsSource()},
            { Pid.SinkAspect, item => item.AsSink()},
            { Pid.ExtractorAspect, item => item.AsExtractor()},
            { Pid.InjectorAspect, item => item.AsInjector()},
            { Pid.ApplierAspect, item => item.AsApplier()},
        };
    }
}
