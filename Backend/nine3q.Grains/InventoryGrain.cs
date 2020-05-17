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
    }

    class InventoryGrain : Grain, IInventory
    {
        private Guid Guid { get; set; }

        private readonly Statistics _stats = new Statistics();
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

        #region Test

        public async Task SetName(string name)
        {
            _state.State.Name = name;
            await _state.WriteStateAsync();
        }

        public Task<string> GetName()
        {
            return Task.FromResult(_state.State.Name);
        }

        #endregion

        #region Interface

        public async Task<long> CreateItem(PropertySet properties)
        {
            _stats.Increment(nameof(CreateItem));
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

        public Task<bool> DeleteItem(long id)
        {
            throw new NotImplementedException();
        }

        public Task<PropertySet> GetItemProperties(long id, PidList pids, bool native = false)
        {
            _stats.Increment(nameof(GetItemProperties));
            return Task.FromResult(Inventory.GetItemProperties(id, pids, native));
        }

        public Task<long> GetItemByName(string name)
        {
            _stats.Increment(nameof(GetItemByName));
            return Task.FromResult(Inventory.GetItemByName(name));
        }

        #endregion

        #region Lifecycle

        public override async Task OnActivateAsync()
        {
            _stats.Increment(nameof(OnActivateAsync));
            _stats.Set("OnActivateAsync.Time", DateTime.UtcNow.ToLong());
            await base.OnActivateAsync();
            _stats.Set("Name", this.GetPrimaryKeyString());

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
            _stats.Increment(nameof(OnDeactivateAsync));
            await base.OnDeactivateAsync();
        }

        async Task CheckInventoryChanged()
        {
            _stats.Increment(nameof(CheckInventoryChanged));
            var summary = new ItemSummaryRecorder(Inventory);

            if (summary.IsChanged()) {
                _stats.Increment($"{nameof(CheckInventoryChanged)}.IsChanged");
                foreach (var id in summary.NewTemplates) {
                    _stats.Increment($"{nameof(CheckInventoryChanged)}.{nameof(InstallTemplate)}");
                    await InstallTemplate(id);
                }

                if (IsPersistent) {
                    _stats.Increment($"{nameof(CheckInventoryChanged)}.{nameof(WriteInventoryState)}");
                    await WriteInventoryState();
                } else {
                    _stats.Increment($"{nameof(CheckInventoryChanged)}.!{nameof(WriteInventoryState)}");
                }

                // Notify subscribers
                var streamProvider = GetStreamProvider(InventoryService.StreamProvider);
                var stream = streamProvider.GetStream<ItemUpdate>(Guid, InventoryService.StreamNamespaceItemUpdate.ToString());
                {
                    var notifyItems = summary.ChangedItems.Clone();
                    notifyItems.UnionWith(summary.AddedItems);
                    foreach (var id in notifyItems) {
                        var parents = Inventory.GetParentContainers(id);
                        var update = new ItemUpdate(id, parents, ItemUpdate.Mode.Changed);
                        _stats.Increment($"{nameof(CheckInventoryChanged)}.{nameof(stream.OnNextAsync)} {nameof(ItemUpdate.Mode.Changed)}");
                        stream.OnNextAsync(update).Ignore();
                    }
                }
                {
                    var notifyItems = summary.DeletedItems;
                    foreach (var id in notifyItems) {
                        var update = new ItemUpdate(id, new ItemIdList(), ItemUpdate.Mode.Removed);
                        _stats.Increment($"{nameof(CheckInventoryChanged)}.{nameof(stream.OnNextAsync)} {nameof(ItemUpdate.Mode.Removed)}");
                        stream.OnNextAsync(update).Ignore();
                    }
                }
            }
        }

        private async Task WriteInventoryState()
        {
            Inventory2State();
            await _state.WriteStateAsync();
        }

        private void Inventory2State()
        {
            _state.State.Name = this.GetPrimaryKeyString();

            // Evaluate Inventory.Changes and prepare only changed items for storage witha specialized InventoryStorageProvider
            // Until then, store complete inventory
            _state.State.Items = new ItemIdPropertiesCollection();
            var ids = Inventory.GetItems();
            foreach (var id in ids) {
                _state.State.Items.Add(id, Inventory.GetItemProperties(id, PidList.All));
            }
        }

        private async Task ReadInventoryState()
        {
            await _state.ReadStateAsync();
            State2Inventory();
        }

        private void State2Inventory()
        {
            //_state.State.Name = this.GetPrimaryKeyString();

            Inventory = new Inventory(this.GetPrimaryKeyString());
        }

        #endregion

        #region Internal

        private async Task InstallTemplate(string name)
        {
            _stats.Increment(nameof(InstallTemplate));
            if (!Templates.IsItem(name)) {
                var inv = GrainFactory.GetGrain<IInventory>(_templatesInventoryName);
                var id = await inv.GetItemByName(name);
                _stats.Increment(nameof(InstallTemplate) + ".GetItemProperties");
                var props = await inv.GetItemProperties(id, PidList.All);
                Templates.CreateItem(props);
            }
        }

        private Task SetPersistent(bool persistent)
        {
            _stats.Increment(nameof(SetPersistent));
            IsPersistent = persistent;
            return Task.CompletedTask;
        }

        #endregion

        #region Test/Maintanance

        public Task Deactivate()
        {
            _stats.Increment("Deactivate");
            base.DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public async Task WritePersistentStorage()
        {
            _stats.Increment("WritePermanentStorage");

            var changes = new List<ItemChange>();
            var ids = Inventory.GetItems();
            foreach (var id in ids) {
                changes.Add(new ItemChange() { What = ItemChange.Variant.TouchItem, ItemId = id });
            }
            Inventory.Changes = changes;

            await WriteInventoryState();
        }

        public async Task ReadPersistentStorage()
        {
            await ReadInventoryState();
        }

        public async Task DeletePersistentStorage()
        {
            _stats.Increment("DeletePermanentStorage");
            await _state.ClearStateAsync();
        }

        #endregion
    }
}
