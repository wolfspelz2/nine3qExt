using n3q.Items;

namespace n3q.Aspects
{
    public static class InjectorExtensions
    {
        public static Injector AsInjector(this ItemStub self) { return new Injector(self); }
    }

    public class Injector : Aspect
    {
        public Injector(ItemStub item) { self = item; }
        public override Pid GetAspectPid() => Pid.InjectorAspect;

        public enum Action { Inject }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.Inject), new ActionDescription() { Handler = async (args) => Inject(await Item(args.Get(Pid.PassiveItem))) } },
            };
        }

        public void Inject(ItemStub passive)
        {
            //sink.AssertAspect(Pid.IsSink);

            //var sinkResource = sink.GetString(Pid.Resource);
            //var sourceResource = this.GetString(Pid.Resource);
            //if (sinkResource != sourceResource) { throw new Exceptions.ResourceMismatchException(Inventory.Name, Id, sink.Id, $"Can not extract: source={sourceResource} vs. sink={sinkResource}"); }
            //Pid levelProperty = Property.Get(sourceResource).Id;
            //Pid maxProperty = Property.GetAssociatedProperty(levelProperty, PropertyName.Modifier.Max);

            //var sinkLevel = sink.GetFloat(levelProperty);
            //var sinkMax = sink.GetFloat(maxProperty);
            //var sourceLevel = this.GetFloat(levelProperty);

            //if (sinkLevel >= sinkMax) { throw new Exceptions.OperationIneffectiveException(Inventory.Name, Id, sink.Id, "Sink is full"); }

            //var transfer = sourceLevel;
            //if (transfer > sinkMax - sinkLevel) {
            //    transfer = sinkMax - sinkLevel;
            //}

            //if (transfer <= 0) { throw new Exceptions.OperationIneffectiveException(Inventory.Name, Id, sink.Id, "No resource transferred"); }

            //sinkLevel += transfer;
            //sourceLevel -= transfer;

            //if (sinkLevel > sinkMax) { sinkLevel = sinkMax; }
            //if (sourceLevel < 0) { sourceLevel = 0; }

            //sink.Set(levelProperty, sinkLevel);
            //this.Set(levelProperty, sourceLevel);

            //if (sourceLevel <= 0) {
            //    if (GetBool(Pid.DeleteExtractedResource)) {
            //        Inventory.DeleteItem(Id);
            //    }
            //}
        }
    }
}
