using n3q.Items;

namespace n3q.Aspects
{
    public static class SourceExtensions
    {
        public static Source AsSource(this ItemStub self) { return new Source(self); }
    }

    public class Source : Aspect
    {
        public Source(ItemStub item) { self = item; }
        public override Pid GetAspectPid() => Pid.SourceAspect;

        public bool IsEmpty()
        {
            return false;
            //string resource = this.GetString(Pid.Resource);
            //if (string.IsNullOrEmpty(resource)) { throw new Exceptions.MissingItemPropertyException(Inventory.Name, this.Id, Pid.Resource); }
            //Pid levelProperty = Property.Get(resource).Id;
            //return GetFloat(levelProperty) == 0;
        }
    }
}
