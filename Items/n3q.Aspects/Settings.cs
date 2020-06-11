using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;

namespace n3q.Aspects
{
    public static class SettingsExtensions
    {
        public static Settings AsSettings(this ItemStub self) { return new Settings(self); }
    }

    public class Settings : Aspect
    {
        public Settings(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.SettingsAspect;

        public enum Action { SetInventoryCoordinates }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.SetInventoryCoordinates), new ActionDescription() { Handler = async (args) => await SetInventoryCoordinates(args.Get(Pid.SettingsSetInventoryCoordinatesLeft), args.Get(Pid.SettingsSetInventoryCoordinatesBottom), args.Get(Pid.SettingsSetInventoryCoordinatesWidth), args.Get(Pid.SettingsSetInventoryCoordinatesHeight)) } },
            };
        }

        public async Task SetInventoryCoordinates(long left, long bottom, long width, long height)
        {
            //await AssertAspect();
            if (left >= 0 && bottom >= 0 && width >= 0 && height >= 0) {
                await this.ModifyProperties(new PropertySet { [Pid.InventoryLeft] = left, [Pid.InventoryBottom] = bottom, [Pid.InventoryWidth] = width, [Pid.InventoryHeight] = height }, PidSet.Empty);
            }
        }
    }
}
