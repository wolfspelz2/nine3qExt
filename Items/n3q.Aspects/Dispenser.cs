using System;
using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;
using n3q.Tools;

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

            var props = await Get(new PidSet { Pid.DispenserAvailable, Pid.DispenserTemplate, Pid.DispenserLastTime, Pid.DispenserCooldownSec, Pid.Container });

            var available = props.GetInt(Pid.DispenserAvailable);
            if (available <= 0) { throw new ItemException(Id, ItemNotification.Fact.NotCreated, ItemNotification.Reason.ItemDepleted); }

            var now = DateTime.MinValue;
            var cooldownSec = props.GetFloat(Pid.DispenserCooldownSec);
            if (cooldownSec > 0) {
                now = await this.AsTimed().CurrentTime();
                var lastTime = props.GetTime(Pid.DispenserLastTime);
                if (now < lastTime.AddSeconds(cooldownSec)) { throw new ItemException(Id, ItemNotification.Fact.NotCreated, ItemNotification.Reason.StillInCooldown); }
            }

            var tmpl = props.GetItemId(Pid.DispenserTemplate);
            if (!Has.Value(tmpl))  { throw new ItemException(Id, Pid.DispenserTemplate, ItemNotification.Fact.NotCreated, ItemNotification.Reason.MissingPropertyValue); }
            var newItem = await this.NewItemFromTemplate(tmpl);
            var containerId = props.GetItemId(Pid.Container);
            if (Has.Value(containerId)) {
                var container = await WritableItem(containerId);
                await container.AsContainer().AddChild(newItem);
            }

            available--;
            await this.Set(Pid.DispenserAvailable, available);

            if (cooldownSec > 0) {
                await this.Set(Pid.DispenserLastTime, now);
            }
        }
    }
}
