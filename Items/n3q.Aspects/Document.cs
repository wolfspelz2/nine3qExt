using n3q.Items;

namespace n3q.Aspects
{
    public static class DocumentExtensions
    {
        public static Document AsDocument(this ItemStub self) { return new Document(self); }
    }

    public class Document : Aspect
    {
        public Document(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.DocumentAspect;
    }
}
