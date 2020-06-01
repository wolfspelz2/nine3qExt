using n3q.Items;

namespace n3q.Aspects
{
    public static class ExtractorExtensions
    {
        public static Extractor AsExtractor(this ItemStub self) { return new Extractor(self); }
    }

    public class Extractor : Aspect
    {
        public Extractor(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.ExtractorAspect;

        public enum Action { Extract }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.Extract), new ActionDescription() { Handler = async (args) => Extract(await Item(args.Get(Pid.ExtractorExtractFrom))) } },
            };
        }

        public void Extract(ItemStub source)
        {
            //source.AssertAspect(Pid.IsSource);

            //var sourceResource = source.GetString(Pid.Resource);
            //var sinkResource = this.GetString(Pid.Resource);
            //if (sinkResource != sourceResource) { throw new Exceptions.ResourceMismatchException(Inventory.Name, Id, source.Id, $"Can not extract: source={sourceResource} vs. sink={sinkResource}"); }
            //Pid levelProperty = Property.Get(sourceResource).Id;
            //Pid maxProperty = Property.GetAssociatedProperty(levelProperty, PropertyName.Modifier.Max);

            //var sinkLevel = this.GetFloat(levelProperty);
            //var sinkMax = this.GetFloat(maxProperty);
            //var sourceLevel = source.GetFloat(levelProperty);

            //if (sinkLevel >= sinkMax) { throw new Exceptions.OperationIneffectiveException(Inventory.Name, Id, source.Id, "Sink is full"); }

            //var transfer = sourceLevel;
            //if (transfer > sinkMax - sinkLevel) {
            //    transfer = sinkMax - sinkLevel;
            //}

            //if (transfer <= 0) { throw new Exceptions.OperationIneffectiveException(Inventory.Name, Id, source.Id, "No resource transferred"); }

            //sinkLevel += transfer;
            //sourceLevel -= transfer;

            //if (sinkLevel > sinkMax) { sinkLevel = sinkMax; }
            //if (sourceLevel < 0) { sourceLevel = 0; }

            //this.Set(levelProperty, sinkLevel);
            //source.Set(levelProperty, sourceLevel);

            //if (sourceLevel <= 0) {
            //    if (source.GetBool(Pid.DeleteExtractedResource)) {
            //        Inventory.DeleteItem(Id);
            //    }
            //}
        }
    }
}
