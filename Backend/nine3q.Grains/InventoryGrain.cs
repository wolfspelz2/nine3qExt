using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using nine3q.Tools;
using nine3q.Items;
using nine3q.Items.Aspects;
using nine3q.GrainInterfaces;
using nine3q.StorageProviders;

namespace nine3q.Grains
{
    [Serializable]
    public class InventoryState
    {
        public string Id;
        public ItemIdPropertiesCollection Items;
        public ItemIdSet WriteIds;
        public ItemIdSet DeleteIds;
    }

    class InventoryGrain : Grain, IInventory, IAsyncObserver<ItemUpdate>
    {
        string Id { get; set; }

        Inventory Inventory;
        bool _isPersistent = true;
        string _streamNamespace = InventoryService.StreamNamespaceDefault;
        string _templatesInventoryName = InventoryService.TemplatesInventoryName;

        readonly Guid _streamId = Guid.NewGuid();
        readonly IPersistentState<InventoryState> _state;

        public InventoryGrain(
            [PersistentState("Inventory", JsonFileStorage.StorageProviderName)] IPersistentState<InventoryState> inventoryState
            )
        {
            _state = inventoryState;
        }

        private IInventory RemoteInventory(string key) => GrainFactory.GetGrain<IInventory>(key);

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

        public Task<long> GetItemByName(string name)
        {
            return Task.FromResult(Inventory.GetItemByName(name));
        }

        public Task<ItemIdSet> GetItemIds()
        {
            return Task.FromResult(Inventory.GetItemIds());
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

        public async Task<int> DeleteItemProperties(long id, PidList pids)
        {
            var deleted = 0;
            Inventory.Transaction(() => {
                deleted = Inventory.DeleteItemProperties(id, pids);
            });
            await CheckInventoryChanged();
            return deleted;
        }

        public async Task ModifyItemProperties(long id, PropertySet modified, PidList deleted)
        {
            Inventory.Transaction(() => {
                Inventory.DeleteItemProperties(id, deleted);
                Inventory.SetItemProperties(id, modified);
            });
            await CheckInventoryChanged();
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

        public async Task<ItemIdPropertiesCollection> BeginItemTransfer(long id)
        {
            var idProps = new ItemIdPropertiesCollection();
            Inventory.Transaction(() => {
                idProps = Inventory.BeginItemTransfer(id);
            });
            await CheckInventoryChanged();
            return idProps;
        }

        public async Task<ItemIdMap> ReceiveItemTransfer(long id, long containerId, long slot, ItemIdPropertiesCollection idProps, PropertySet finallySetProperties, PidList finallyDeleteProperties)
        {
            var mapping = new ItemIdMap();
            Inventory.Transaction(() => {
                mapping = Inventory.ReceiveItemTransfer(id, containerId, slot, idProps, finallySetProperties, finallyDeleteProperties);
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

        public Task SetPersistent(bool persistent)
        {
            _isPersistent = persistent;
            return Task.CompletedTask;
        }

        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }
        public Task<string> GetStreamNamespace() { return Task.FromResult(_streamNamespace); }

        public Task SetStreamNamespace(string ns)
        {
            _streamNamespace = ns;
            return Task.CompletedTask;
        }

        #endregion

        #region Lifecycle

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            Id = this.GetPrimaryKeyString();

            await ReadInventoryState();

            //Inventory.Timers = new InventoryGrainTimerManager(this);

            if (Id == _templatesInventoryName) {
                Inventory.IsActive = false;
            } else {
                await PopulateUsedTemplates();
                await ActivateTemplateSubscription();
                Inventory.Activate();
            }
        }

        async Task PopulateUsedTemplates()
        {
            var templateInv = GrainFactory.GetGrain<IInventory>(_templatesInventoryName);
            foreach (var pair in Inventory.Items) {
                var item = pair.Value;
                var templateName = item.GetString(Pid.TemplateName);
                if (!string.IsNullOrEmpty(templateName)) {
                    await InstallTemplate(templateName);
                }
            }
        }

        async Task InstallTemplate(string name)
        {
            Inventory.Templates ??= new Inventory();
            if (!Inventory.Templates.IsItem(name)) {
                var inv = RemoteInventory(_templatesInventoryName);
                var id = await inv.GetItemByName(name);
                if (id != ItemId.NoItem) {
                    var props = await inv.GetItemProperties(id, PidList.All);
                    Inventory.Templates.CreateItem(props);
                }
            }
        }

        private async Task ActivateTemplateSubscription()
        {
            var streamProvider = GetStreamProvider(InventoryService.StreamProvider);
            var templatesStreamId = await RemoteInventory(_templatesInventoryName).GetStreamId();
            var templatesStreamNamespace = await RemoteInventory(_templatesInventoryName).GetStreamNamespace();
            var templatesStream = streamProvider.GetStream<ItemUpdate>(templatesStreamId, templatesStreamNamespace);
            var handles = await templatesStream.GetAllSubscriptionHandles();
            if (handles.Count == 0) {
                var handle = await templatesStream.SubscribeAsync((data, token) => OnTemplateUpdate(data));
            } else {
                foreach (var handle in handles) {
                    await handle.ResumeAsync((data, token) => OnTemplateUpdate(data));
                }
            }
        }

        private async Task DeactivateTemplateSubscription()
        {
            var streamProvider = GetStreamProvider(InventoryService.StreamProvider);
            var templatesGuid = await RemoteInventory(_templatesInventoryName).GetStreamId();
            var templatesStreamNamespace = await RemoteInventory(_templatesInventoryName).GetStreamNamespace();
            var templatesStream = streamProvider.GetStream<ItemUpdate>(templatesGuid, templatesStreamNamespace);
            var handles = await templatesStream.GetAllSubscriptionHandles();
            foreach (var handle in handles) {
                await handle.UnsubscribeAsync();
            }
        }

        public async Task OnNextAsync(ItemUpdate itemUpdate, StreamSequenceToken token = null)
        {
            await OnTemplateUpdate(itemUpdate);
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public async Task OnTemplateUpdate(ItemUpdate update)
        {
            Inventory.Templates ??=  new Inventory();
            var templateId = update.Id;
            if (Inventory.Templates.IsItem(templateId)) {
                var template = Inventory.Templates.Item(templateId);
                switch (update.What) {
                    case ItemUpdate.Mode.Added:
                        template.Delete(Pid.StaleTemplate);
                        break;
                    case ItemUpdate.Mode.Changed:
                        var templateInv = GrainFactory.GetGrain<IInventory>(_templatesInventoryName);
                        var changedTemplateProps = await templateInv.GetItemProperties(templateId, PidList.All);
                        template.Properties = changedTemplateProps;
                        break;
                    case ItemUpdate.Mode.Removed:
                        template.SetBool(Pid.StaleTemplate, true);
                        break;
                }
            }
        }

        public override async Task OnDeactivateAsync()
        {
            await base.OnDeactivateAsync();
        }

        #endregion

        #region Changes

        async Task CheckInventoryChanged()
        {
            var summary = new ItemChangesSummary(Inventory);

            if (summary.IsChanged()) {
                foreach (var id in summary.NewTemplates) {
                    await InstallTemplate(id);
                }

                if (_isPersistent) {
                    await WriteInventoryState(summary);
                }

                // Notify subscribers
                {
                    var streamProvider = GetStreamProvider(InventoryService.StreamProvider);
                    var stream = streamProvider.GetStream<ItemUpdate>(_streamId, _streamNamespace);
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
                }
            }
        }

        async Task WriteInventoryState(ItemChangesSummary summary)
        {
            _state.State.Id = Id;

            Don.t = () => {
                _state.State.WriteIds = summary.ChangedItems.Clone();
                _state.State.WriteIds.UnionWith(summary.AddedItems);
                _state.State.DeleteIds = summary.DeletedItems;

                var itemIds = _state.State.WriteIds;
            };
            var itemIds = Inventory.GetItemIds();

            _state.State.Items = new ItemIdPropertiesCollection();
            foreach (var id in itemIds) {
                _state.State.Items.Add(id, Inventory.GetItemProperties(id, PidList.All, native: true));
            }

            await _state.WriteStateAsync();
        }

        async Task ReadInventoryState()
        {
            await _state.ReadStateAsync();

            Inventory = new Inventory(Id);

            var allProps = _state.State.Items;
            if (allProps != null) {
                foreach (var pair in allProps) {
                    Inventory.CreateItem(pair.Value);
                }
            }
        }

        #endregion

        #region Test/Maintanance

        public async Task SetTemplateInventoryName(string name)
        {
            await DeactivateTemplateSubscription();
            _templatesInventoryName = name;
            await PopulateUsedTemplates();
            await ActivateTemplateSubscription();
        }

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

            await WriteInventoryState(new ItemChangesSummary(Inventory));
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
