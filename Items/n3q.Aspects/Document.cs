using System;
using System.Threading.Tasks;
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

        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(SetText), new ActionDescription() { Handler = async (args) => await SetText(args.Get(Pid.DocumentSetTextText)) } },
            };
        }

        public async Task SetText(string text)
        {
            await AssertAspect(Pid.DocumentAspect);

            var len = text.Length;
            var maxLen = await this.GetInt(Pid.DocumentMaxLength);

            if (len > maxLen) { throw new Exception($"Max text length {maxLen} exceeded"); }

            await this.Set(Pid.DocumentText, text);
        }
    }
}
