using n3q.Items;

namespace n3q.Aspects
{
    public static class SinkExtensions
    {
        public static Sink AsSink(this ItemStub self) { return new Sink(self); }
    }

    public class Sink : Aspect
    {
        public Sink(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.SinkAspect;

        public bool IsFull()
        {
            return false;
            //string resource = this.GetString(Pid.Resource);
            //if (string.IsNullOrEmpty(resource)) { throw new Exceptions.MissingItemPropertyException(Inventory.Name, this.Id, Pid.Resource); }
            //Pid levelProperty = Property.Get(resource).Id;
            //Pid maxProperty = Property.GetAssociatedProperty(levelProperty, PropertyName.Modifier.Max);
            //return GetFloat(levelProperty) >= GetFloat(maxProperty);
        }
    }
}
