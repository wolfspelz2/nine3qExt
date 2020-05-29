﻿using n3q.Items;

namespace n3q.Aspects
{
    public static class ApplierExtensions
    {
        public static Applier AsApplier(this ItemStub self) { return new Applier(self); }
    }

    public class Applier : Aspect
    {
        public Applier(ItemStub item) { self = item; }
        public override Pid GetAspectPid() => Pid.ApplierAspect;

        public enum Action { ApplyTo }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.ApplyTo), new ActionDescription() { Handler = async (args) => ApplyTo(await Item(args.Get(Pid.PassiveItem))) } },
            };
        }

        public void ApplyTo(ItemStub passive)
        {
            //if (this.IsExtractor() && passive.IsSource()) {
            //    if (!this.AsSink().IsFull() && !passive.AsSource().IsEmpty() && this.GetString(Pid.Resource) == passive.GetString(Pid.Resource)) {
            //        this.AsExtractor().Extract(passive);
            //        return;
            //    }
            //}

            //if (this.IsInjector() && passive.IsSink()) {
            //    if (!this.AsSource().IsEmpty() && !passive.AsSink().IsFull() && this.GetString(Pid.Resource) == passive.GetString(Pid.Resource)) {
            //        this.AsInjector().Inject(passive);
            //        return;
            //    }
            //}
        }
    }
}
