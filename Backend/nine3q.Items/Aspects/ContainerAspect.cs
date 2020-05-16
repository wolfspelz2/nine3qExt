using System.Collections.Generic;
using nine3q.Items.Exceptions;

namespace nine3q.Items.Aspects
{
    public static class ContainerAspectExtensions
    {
        public static ContainerAspect AsContainer(this Item self) { self.AssertAspect(Pid.IsContainer); return new ContainerAspect(self); }
        public static bool IsContainer(this Item self) { return self.IsAspect(Pid.IsContainer); }
    }

    public class ContainerAspect : Aspect
    {
        public ContainerAspect(Item self) : base(self) { }

        public enum Action { SetChild, RemoveChild }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { Action.SetChild.ToString(), new ActionDescription() { Handler = (args) => SetChild(Inventory.Item(args.GetItem(Pid.Item)), args.GetInt(Pid.Slot)) } },
                { Action.RemoveChild.ToString(), new ActionDescription() { Handler = (args) => RemoveChild(Inventory.Item(args.GetItem(Pid.Item))) } },
            };
        }
        public const long NoSlot = 0;

        internal override void OnAspectActivate()
        {
            foreach (var child in this.AsContainer().GetChildren()) {
                child.Activate();
            }
        }

        internal override void OnAspectDelete()
        {
            DeleteChildren();
        }

        public void AddChildCore(Item item)
        {
            var currentContainerId = item.GetItem(Pid.Container);
            if (currentContainerId != ItemId.NoItem) {
                if (currentContainerId == this.Id) {
                    return;
                } else {
                    Inventory.Item(currentContainerId).AsContainer().RemoveChild(item);
                }
            }
            this.AddToItemSet(Pid.Contains, item.Id);
            item.Set(Pid.Container, this.Id);
        }

        public void AssigneSlotCore(Item item, long slot = NoSlot)
        {
            item.Delete(Pid.Slot);
            if (slot != NoSlot) {
                if (!CanPlaceAt(item, slot)) { throw new SlotAvailabilityException(Inventory.Name, Id, item.Id, $"Can not place at desired slot={slot}"); }
            } else {
                slot = GetFreeSlot(item);
            }
            item.Set(Pid.Slot, slot);
        }

        public void AddChild(Item item, long slot = NoSlot)
        {
            AddChildCore(item);
            AssigneSlotCore(item, slot);
        }

        public void SetChild(Item item, long slot = NoSlot)
        {
            if (this.AsContainer().IsChild(item)) {
                if (slot != NoSlot && slot == GetInt(Pid.Slot)) { throw new OperationIneffectiveException(Inventory.Name, Id, item.Id, "Same slot"); }
                if (!GetBool(Pid.ContainerCanShuffle)) { throw new MissingItemPropertyException(Inventory.Name, Id, Pid.ContainerCanShuffle); }
            } else {
                if (!GetBool(Pid.ContainerCanImport)) { throw new MissingItemPropertyException(Inventory.Name, Id, Pid.ContainerCanImport); }
                var currentContainerId = item.GetItem(Pid.Container);
                if (currentContainerId != ItemId.NoItem) {
                    if (!Inventory.Item(currentContainerId).GetBool(Pid.ContainerCanExport)) { throw new MissingItemPropertyException(Inventory.Name, currentContainerId, Pid.ContainerCanExport); }
                }
            }

            AddChild(item, slot);
        }

        public void RemoveChild(Item item)
        {
            this.RemoveFromItemSet(Pid.Contains, item.Id);
            item.Delete(Pid.Container);
            item.Delete(Pid.Slot);
        }

        public void DeleteChildren()
        {
            var childrenIds = GetItemSet(Pid.Contains).Clone(); // Clone to change list while looping
            if (childrenIds.Count > 0) {
                foreach (var childId in childrenIds) {
                    var child = Inventory.Item(childId);
                    Inventory.DeleteItem(childId);
                }
            }
        }

        public List<Item> GetChildren()
        {
            var children = new List<Item>();

            foreach (var childId in GetItemSet(Pid.Contains)) {
                var child = Inventory.Item(childId);
                children.Add(child);
            }

            return children;
        }

        public bool IsChild(Item item)
        {
            return GetItemSet(Pid.Contains).Contains(item.Id);
        }

        protected bool CanPlaceAt(Item item, long slot)
        {
            if (slot == NoSlot) { throw new SlotAvailabilityException(Inventory.Name, this.Id, item.Id, $"Can not place at slot={slot}"); }
            if (slot > GetInt(Pid.Slots)) { throw new SlotAvailabilityException(Inventory.Name, this.Id, item.Id, $"Can not place at slot={slot} because Repository has only {GetInt(Pid.Slots)} slots"); }

            foreach (var child in this.AsContainer().GetChildren()) {
                if (slot == child.GetInt(Pid.Slot)) {
                    return false;
                }
            }

            return true;
        }

        public long GetFreeSlot(Item item)
        {
            long result = FindFreeSlot(item);
            if (result == NoSlot) { throw new SlotAvailabilityException(Inventory.Name, this.Id, item.Id, "No free slot"); }
            return result;
        }

        protected long FindFreeSlot(Item item)
        {
            long result = NoSlot;

            var orderedUsedSlots = new SortedList<long, ItemId>();
            foreach (var child in this.AsContainer().GetChildren()) {
                var slot = child.GetInt(Pid.Slot);
                if (slot != NoSlot) {
                    orderedUsedSlots.Add(slot, child.Id);
                }
            }

            var slots = GetInt(Pid.Slots);
            for (long candidate = 1; candidate <= slots; candidate++) {
                if (!orderedUsedSlots.ContainsKey(candidate)) {
                    var capacity = 1; //constraints.GetCapacityForSlot(this, item, nCandidate);
                    if (capacity >= item.GetInt(Pid.Stacksize)) {
                        result = candidate;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
