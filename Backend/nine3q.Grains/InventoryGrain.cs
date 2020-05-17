using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using nine3q.GrainInterfaces;
using Orleans.Providers;
using nine3q.Tools;
using nine3q.Items;
using nine3q.Items.Aspects;

namespace nine3q.Grains
{
    public static class InventoryService
    {
        public const string TemplatesInventoryName = "Templates";
        public const string StorageProvider = "InventoryStorage";
        public const string StreamProvider = "SMSProvider";
        public const string StreamNamespaceItemUpdate = "SMSProvider";
    }

    public class InventoryState
    {
        public Inventory Inventory { get; set; }
    }

    [StorageProvider(ProviderName = InventoryService.StorageProvider)]
    class InventoryGrain : Grain<InventoryState>, IInventory
    {
        private string Name { get; set; }
        private Guid Guid { get; set; }

        private readonly Statistics _stats = new Statistics();
        private readonly string _templatesInventoryName = Grains.InventoryService.TemplatesInventoryName;

        private Inventory Inventory { get { return State.Inventory; } set { State.Inventory = value; } }
        private readonly Inventory Templates = new Inventory();

        private bool IsPersistent { get; set; } = true;

        #region Interface

        public async Task<ItemId> CreateItem(PropertySet properties)
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

        public Task<bool> DeleteItem(ItemId id)
        {
            throw new NotImplementedException();
        }

        public Task<PropertySet> GetItemProperties(ItemId id, PidList pids, bool native = false)
        {
            _stats.Increment(nameof(GetItemProperties));
            return Task.FromResult(Inventory.GetItemProperties(id, pids, native));
        }

        public Task<ItemId> GetItemByName(string name)
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

            Name = this.GetPrimaryKeyString();
            _stats.Set("Name", Name);
            Guid = NameBasedGuid.Create(NameBasedGuid.UrlNamespace, Name);

            if (Inventory == null) { Inventory = new Inventory(Name); }
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
                _stats.Increment(nameof(CheckInventoryChanged) + ".IsChanged");
                foreach (var id in summary.NewTemplates) {
                    _stats.Increment(nameof(CheckInventoryChanged) + ".InstallTemplate");
                    await InstallTemplate(id);
                }

                if (IsPersistent) {
                    _stats.Increment(nameof(CheckInventoryChanged) + ".WriteStateAsync");
                    await base.WriteStateAsync();
                } else {
                    _stats.Increment(nameof(CheckInventoryChanged) + ".!WriteStateAsync");
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
                        _stats.Increment(nameof(CheckInventoryChanged) + ".OnNextAsync");
                        stream.OnNextAsync(update).Ignore();
                    }
                }
                {
                    var notifyItems = summary.DeletedItems;
                    foreach (var id in notifyItems) {
                        var update = new ItemUpdate(id, new ItemIdList(), ItemUpdate.Mode.Removed);
                        _stats.Increment(nameof(CheckInventoryChanged) + ".OnNextAsync");
                        stream.OnNextAsync(update).Ignore();
                    }
                }
            }
        }

        #endregion

        #region Private

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

        public async Task DeletePermanentStorage()
        {
            _stats.Increment("DeletePermanentStorage");
            await base.ClearStateAsync();
        }

        #endregion
    }
}
