using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;

namespace n3q.Aspects
{
    public static class DispenserExtensions
    {
        public static Dispenser AsDispenser(this ItemStub self) { return new Dispenser(self); }
    }

    public class Dispenser : Aspect
    {
        public Dispenser(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.DispenserAspect;

        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(GetItem), new ActionDescription() { Handler = async (args) => await GetItem() } },
            };
        }

        public async Task GetItem()
        {
            await AssertAspect();

            var props = await Get(new PidSet { Pid.DispenserAvailable, Pid.DispenserTemplate, Pid.Container });
            var available = props.GetInt(Pid.DispenserAvailable);
            if (available <= 0) { throw new ItemException(Id, ItemNotification.Fact.NotCreated, ItemNotification.Reason.ItemDepleted);  }
            var tmpl = props.GetItemId(Pid.DispenserTemplate);
            var newItem = await this.NewItemFromTemplate(tmpl);
            var container = await WritableItem(props.GetItemId(Pid.Container));
            await container.AsContainer().AddChild(newItem);
        }
    }
}
