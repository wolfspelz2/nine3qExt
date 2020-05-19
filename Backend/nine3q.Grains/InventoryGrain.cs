using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using nine3q.Tools;
using nine3q.Items;
using nine3q.Items.Aspects;
using nine3q.GrainInterfaces;
using nine3q.StorageProviders;

namespace nine3q.Grains
{
    public static class InventoryService
    {
        public const string TemplatesInventoryName = "Templates";
        public const string StreamProvider = "SMSProvider";
        public const string StreamNamespaceItemUpdate = "SMSProvider";
    }

    [Serializable]
    public class InventoryState
    {
        public string Name;
        public ItemIdPropertiesCollection Items;
        public ItemIdSet WriteIds;
        public ItemIdSet DeleteIds;
    }

    class InventoryGrain : Grain, IInventory
    {
        private Guid Guid { get; set; }

        private readonly string _templatesInventoryName = Grains.InventoryService.TemplatesInventoryName;

        private readonly IPersistentState<InventoryState> _state;
        private Inventory Inventory;

        private readonly Inventory Templates = new Inventory();

        private bool IsPersistent { get; set; } = true;

        public InventoryGrain(
            [PersistentState("Inventory", JsonFileStorage.StorageProviderName)] IPersistentState<InventoryState> inventoryState
            )
        {
            _state = inventoryState;
        }

        #region Interface

        public async Task<long> CreateItem(PropertySet properties)
        {
            Item item = null;
            Inventory.Transaction(() => {
                var slot = properties.GetInt(Pid.Slot);
                var containerId = properties.GetItem(Pid.Container);

                properties.Remove(Pid.Slot);
                properties.Remove(Pid.Container);
                properties.Remove(Pid.Contains);

                item = Inventory.CreateItem(properties);

                if (containerId != ItemId.NoItem) {
                    var container = Inventory.Item(containerId);
                    container.AsContainer().AddChild(item, slot);
                }
            });
            await CheckInventoryChanged();
            return item.Id;

            //await Task.CompletedTask;
            //return ItemId.NoItem;
        }

        public async Task<bool> DeleteItem(long id)
        {
            var deleted = true;
            Inventory.Transaction(() => {
                deleted = Inventory.DeleteItem(id);
            });
            await CheckInventoryChanged();
            return deleted;
        }

        public async Task SetItemProperties(long id, PropertySet properties)
        {
            Inventory.Transaction(() => {
                Inventory.SetItemProperties(id, properties);
            });
            await CheckInventoryChanged();
        }

        public Task<PropertySet> GetItemProperties(long id, PidList pids, bool native = false)
        {
            return Task.FromResult(Inventory.GetItemProperties(id, pids, native));
        }

        public Task<long> GetItemByName(string name)
        {
            return Task.FromResult(Inventory.GetItemByName(name));
        }

        public Task<ItemIdSet> GetItemIds()
        {
            return Task.FromResult(Inventory.GetItemIds());
        }

        public Task<ItemIdPropertiesCollection> GetItemIdsAndValuesByProperty(Pid filterPid, PidList desiredProperties)
        {
            return Task.FromResult(Inventory.GetItemIdsAndValuesByProperty(filterPid, desiredProperties));
        }

        public async Task AddChildToContainer(long id, long containerId, long slot)
        {
            Inventory.Transaction(() => {
                Inventory.AddChild(containerId, id, slot);
            });
            await CheckInventoryChanged();
        }

        public async Task RemoveChildFromContainer(long id, long containerId)
        {
            Inventory.Transaction(() => {
                Inventory.RemoveChild(containerId, id);
            });
            await CheckInventoryChanged();
        }

        #endregion

        #region Lifecycle

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            Guid = NameBasedGuid.Create(NameBasedGuid.UrlNamespace, this.GetPrimaryKeyString());

            await ReadInventoryState();

            //Inventory.Timers = new InventoryGrainTimerManager(this);

            //if (Name == _templatesInventoryName) {
            //    Inventory.IsActive = false;
            //} else {
            //    await PopulateUsedTemplates();
            //    await ActivateTemplateSubscription();
            //    Inventory.Activate();
            //}
        }

        public override async Task OnDeactivateAsync()
        {
            await base.OnDeactivateAsync();
        }

        async Task CheckInventoryChanged()
        {
            var summary = new ItemSummaryRecorder(Inventory);

            if (summary.IsChanged()) {
                foreach (var id in summary.NewTemplates) {
                    await InstallTemplate(id);
                }

                if (IsPersistent) {
                    await WriteInventoryState(summary);
                }

                // Notify subscribers
                Misc.Dont = () => {
                    var streamProvider = GetStreamProvider(InventoryService.StreamProvider);
                    var stream = streamProvider.GetStream<ItemUpdate>(Guid, InventoryService.StreamNamespaceItemUpdate.ToString());
                    {
                        var notifyItems = summary.ChangedItems.Clone();
                        notifyItems.UnionWith(summary.AddedItems);
                        foreach (var id in notifyItems) {
                            var parents = Inventory.GetParentContainers(id);
                            var update = new ItemUpdate(id, parents, ItemUpdate.Mode.Changed);
                            stream.OnNextAsync(update).Ignore();
                        }
                    }
                    {
                        var notifyItems = summary.DeletedItems;
                        foreach (var id in notifyItems) {
                            var update = new ItemUpdate(id, new ItemIdSet(), ItemUpdate.Mode.Removed);
                            stream.OnNextAsync(update).Ignore();
                        }
                    }
                };
            }
        }

        private async Task WriteInventoryState(ItemSummaryRecorder summary)
        {
            _state.State.Name = this.GetPrimaryKeyString();

            Misc.Dont = () => {
                _state.State.WriteIds = summary.ChangedItems.Clone();
                _state.State.WriteIds.UnionWith(summary.AddedItems);
                _state.State.DeleteIds = summary.DeletedItems;

                var itemIds = _state.State.WriteIds;
            };
            var itemIds = Inventory.GetItemIds();

            _state.State.Items = new ItemIdPropertiesCollection();
            foreach (var id in itemIds) {
                _state.State.Items.Add(id, Inventory.GetItemProperties(id, PidList.All));
            }

            await _state.WriteStateAsync();
        }

        private async Task ReadInventoryState()
        {
            await _state.ReadStateAsync();

            Inventory = new Inventory(this.GetPrimaryKeyString());

            var allProps = _state.State.Items;
            if (allProps != null) {
                foreach (var pair in allProps) {
                    Inventory.CreateItem(pair.Value);
                }
            }
        }

        #endregion

        #region Transfer

        public async Task<ItemIdPropertiesCollection> BeginItemTransfer(long id)
        {
            var idProps = new ItemIdPropertiesCollection();
            Inventory.Transaction(() => {
                idProps = Inventory.BeginItemTransfer(id);
            });
            await CheckInventoryChanged();
            return idProps;
        }

        public async Task<ItemIdMap> ReceiveItemTransfer(long id, long containerId, long slot, ItemIdPropertiesCollection idProps, PropertySet setProperties, PidList removeProperties)
        {
            var mapping = new ItemIdMap();
            Inventory.Transaction(() => {
                mapping = Inventory.ReceiveItemTransfer(id, containerId, slot, idProps, setProperties, removeProperties);
            });
            await CheckInventoryChanged();
            return mapping;
        }

        public async Task EndItemTransfer(long id)
        {
            Inventory.Transaction(() => {
                Inventory.EndItemTransfer(id);
            });
            await CheckInventoryChanged();
        }

        public async Task CancelItemTransfer(long id)
        {
            Inventory.Transaction(() => {
                Inventory.CancelItemTransfer(id);
            });
            await CheckInventoryChanged();
        }

        #endregion

        #region Internal

        private async Task InstallTemplate(string name)
        {
            if (!Templates.IsItem(name)) {
                var inv = GrainFactory.GetGrain<IInventory>(_templatesInventoryName);
                var id = await inv.GetItemByName(name);
                var props = await inv.GetItemProperties(id, PidList.All);
                Templates.CreateItem(props);
            }
        }

        public Task SetPersistent(bool persistent)
        {
            IsPersistent = persistent;
            return Task.CompletedTask;
        }

        #endregion

        #region Test/Maintanance

        public Task Deactivate()
        {
            base.DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public async Task WritePersistentStorage()
        {
            var changes = new List<ItemChange>();
            var ids = Inventory.GetItemIds();
            foreach (var id in ids) {
                changes.Add(new ItemChange() { What = ItemChange.Variant.TouchItem, ItemId = id });
            }
            Inventory.Changes = changes;

            await WriteInventoryState(new ItemSummaryRecorder(Inventory));
        }

        public async Task ReadPersistentStorage()
        {
            await ReadInventoryState();
        }

        public async Task DeletePersistentStorage()
        {
            await _state.ClearStateAsync();
        }

        #endregion
    }
}
