using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
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
        public string StreamNamespace;
        public long LastItemId;
        public ItemIdPropertiesCollection Items;
        public ItemIdSet WriteIds;
        public ItemIdSet DeleteIds;
    }

    class InventoryGrain : Grain, IInventory, IAsyncObserver<ItemUpdate>
    {
        string Id { get; set; }

        Inventory _inventory;
        bool _isPersistent = true;
        string _streamNamespace = InventoryService.StreamNamespaceDefault;
        string _templatesInventoryName = InventoryService.TemplatesInventoryName;

        readonly Guid _streamId = InventoryService.StreamGuidDefault;
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
            _inventory.Transaction(() => {
                var slot = properties.GetInt(Pid.Slot);
                var containerId = properties.GetItem(Pid.Container);

                properties.Remove(Pid.Slot);
                properties.Remove(Pid.Container);
                properties.Remove(Pid.Contains);

                item = _inventory.CreateItem(properties);

                if (containerId != ItemId.NoItem) {
                    var container = _inventory.Item(containerId);
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
            _inventory.Transaction(() => {
                deleted = _inventory.DeleteItem(id);
            });
            await CheckInventoryChanged();
            return deleted;
        }

        public Task<long> GetItemByName(string name)
        {
            return Task.FromResult(_inventory.GetItemByName(name));
        }

        public Task<ItemIdSet> GetItemIds()
        {
            return Task.FromResult(_inventory.GetItemIds());
        }

        public async Task SetItemProperties(long id, PropertySet properties)
        {
            _inventory.Transaction(() => {
                _inventory.SetItemProperties(id, properties);
            });
            await CheckInventoryChanged();
        }

        public Task<PropertySet> GetItemProperties(long id, PidList pids, bool native = false)
        {
            return Task.FromResult(_inventory.GetItemProperties(id, pids, native));
        }

        public async Task<int> DeleteItemProperties(long id, PidList pids)
        {
            var deleted = 0;
            _inventory.Transaction(() => {
                deleted = _inventory.DeleteItemProperties(id, pids);
            });
            await CheckInventoryChanged();
            return deleted;
        }

        public async Task ModifyItemProperties(long id, PropertySet modified, PidList deleted)
        {
            _inventory.Transaction(() => {
                _inventory.DeleteItemProperties(id, deleted);
                _inventory.SetItemProperties(id, modified);
            });
            await CheckInventoryChanged();
        }

        public Task<ItemIdPropertiesCollection> GetItemIdsAndValuesByProperty(Pid filterPid, PidList desiredProperties)
        {
            return Task.FromResult(_inventory.GetItemIdsAndValuesByProperty(filterPid, desiredProperties));
        }

        public async Task AddChildToContainer(long id, long containerId, long slot)
        {
            _inventory.Transaction(() => {
                _inventory.AddChild(containerId, id, slot);
            });
            await CheckInventoryChanged();
        }

        public async Task RemoveChildFromContainer(long id, long containerId)
        {
            _inventory.Transaction(() => {
                _inventory.RemoveChild(containerId, id);
            });
            await CheckInventoryChanged();
        }

        public async Task<ItemIdPropertiesCollection> BeginItemTransfer(long id)
        {
            var idProps = new ItemIdPropertiesCollection();
            _inventory.Transaction(() => {
                idProps = _inventory.BeginItemTransfer(id);
            });
            await CheckInventoryChanged();
            return idProps;
        }

        public async Task<ItemIdMap> ReceiveItemTransfer(long id, long containerId, long slot, ItemIdPropertiesCollection idProps, PropertySet finallySetProperties, PidList finallyDeleteProperties)
        {
            var mapping = new ItemIdMap();
            _inventory.Transaction(() => {
                mapping = _inventory.ReceiveItemTransfer(id, containerId, slot, idProps, finallySetProperties, finallyDeleteProperties);
            });
            await CheckInventoryChanged();
            return mapping;
        }

        public async Task EndItemTransfer(long id)
        {
            _inventory.Transaction(() => {
                _inventory.EndItemTransfer(id);
            });
            await CheckInventoryChanged();
        }

        public async Task CancelItemTransfer(long id)
        {
            _inventory.Transaction(() => {
                _inventory.CancelItemTransfer(id);
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

        public async Task SetStreamNamespace(string ns)
        {
            _streamNamespace = ns;
            await WritePersistentStorage();
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
                _inventory.IsActive = false;
            } else {
                await PopulateUsedTemplates();
                await ActivateTemplateSubscription();
                _inventory.Activate();
            }
        }

        async Task PopulateUsedTemplates()
        {
            var templateInv = GrainFactory.GetGrain<IInventory>(_templatesInventoryName);
            foreach (var pair in _inventory.Items) {
                var item = pair.Value;
                var templateName = item.GetString(Pid.TemplateName);
                if (!string.IsNullOrEmpty(templateName)) {
                    await InstallTemplate(templateName);
                }
            }
        }

        async Task InstallTemplate(string name)
        {
            _inventory.Templates ??= new Inventory();
            if (!_inventory.Templates.IsItem(name)) {
                var inv = RemoteInventory(_templatesInventoryName);
                var id = await inv.GetItemByName(name);
                if (id != ItemId.NoItem) {
                    var props = await inv.GetItemProperties(id, PidList.All);
                    _inventory.Templates.CreateItem(props);
                }
            }
        }

        private async Task ActivateTemplateSubscription()
        {
            var templatesStream = await GetTemplatesStream();
            var handles = await templatesStream.GetAllSubscriptionHandles();
            if (handles.Count == 0) {
                var handle = await templatesStream.SubscribeAsync(this);
            } else {
                foreach (var handle in handles) {
                    await handle.ResumeAsync(this);
                }
            }
        }

        private async Task DeactivateTemplateSubscription()
        {
            var templatesStream = await GetTemplatesStream();
            var handles = await templatesStream.GetAllSubscriptionHandles();
            foreach (var handle in handles) {
                await handle.UnsubscribeAsync();
            }
        }

        private async Task<IAsyncStream<ItemUpdate>> GetTemplatesStream()
        {
            var streamProvider = GetStreamProvider(InventoryService.StreamProvider);
            var templatesGuid = await RemoteInventory(_templatesInventoryName).GetStreamId();
            var templatesStreamNamespace = await RemoteInventory(_templatesInventoryName).GetStreamNamespace();
            var templatesStream = streamProvider.GetStream<ItemUpdate>(templatesGuid, templatesStreamNamespace);
            return templatesStream;
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
            _inventory.Templates ??= new Inventory();
            var templateId = update.Id;
            if (_inventory.Templates.IsItem(templateId)) {
                var template = _inventory.Templates.Item(templateId);
                switch (update.What) {
                    case ItemUpdate.Mode.Added:
                        template.Delete(Pid.StaleTemplate);
                        break;
                    case ItemUpdate.Mode.Changed:
                        var templateInv = GrainFactory.GetGrain<IInventory>(_templatesInventoryName);
                        var newTemplateProps = await templateInv.GetItemProperties(templateId, PidList.All);
                        //await ForwardItemUpdateforAffectedItems(update, newTemplateProps);
                        template.Properties = newTemplateProps;
                        break;
                    case ItemUpdate.Mode.Removed:
                        template.SetBool(Pid.StaleTemplate, true);
                        break;
                }
            }
        }

        //public async Task ForwardItemUpdateforAffectedItems(ItemUpdate update, PropertySet newTemplateProps)
        //{
        //    var templateName = newTemplateProps.GetString(Pid.Name);
        //    foreach (var pair in _inventory.Items) {
        //        var itemId = pair.Key;
        //        var item = pair.Value;
        //        if (item.GetString(Pid.TemplateName) == templateName) {
        //            foreach (var pid in update.Pids) {
        //                var itemChange = new ItemChange { What = ItemChange.Variant., ItemId = id };


        //            }
        //        }
        //    }
        //}

        public override async Task OnDeactivateAsync()
        {
            await base.OnDeactivateAsync();
        }

        #endregion

        #region Changes

        async Task CheckInventoryChanged()
        {
            var summary = new ItemChangesSummary(_inventory);

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
                            var parents = _inventory.GetParentContainers(id);
                            var pids = summary.ChangedItemsProperties.ContainsKey(id) ? summary.ChangedItemsProperties[id] : PidList.Empty;
                            var update = new ItemUpdate(Id, id, pids, parents, ItemUpdate.Mode.Changed);
                            await stream.OnNextAsync(update);
                        }
                    }
                    {
                        var notifyItems = summary.DeletedItems;
                        foreach (var id in notifyItems) {
                            var update = new ItemUpdate(Id, id, PidList.Empty, new ItemIdSet(), ItemUpdate.Mode.Removed);
                            await stream.OnNextAsync(update);
                        }
                    }
                }
            }
        }

        async Task WriteInventoryState(ItemChangesSummary summary)
        {
            _state.State.Id = Id;
            _state.State.StreamNamespace = _streamNamespace;
            _state.State.LastItemId = _inventory.GetLastItemId();

            Don.t = () => {
                _state.State.WriteIds = summary.ChangedItems.Clone();
                _state.State.WriteIds.UnionWith(summary.AddedItems);
                _state.State.DeleteIds = summary.DeletedItems;

                var itemIds = _state.State.WriteIds;
            };
            var itemIds = _inventory.GetItemIds();

            _state.State.Items = new ItemIdPropertiesCollection();
            foreach (var id in itemIds) {
                _state.State.Items.Add(id, _inventory.GetItemProperties(id, PidList.All, native: true));
            }

            await _state.WriteStateAsync();
        }

        async Task ReadInventoryState()
        {
            await _state.ReadStateAsync();

            _inventory = new Inventory(Id);

            var allProps = _state.State.Items;
            if (allProps != null) {
                foreach (var pair in allProps) {
                    _inventory.CreateItem(pair.Value);
                }
            }

            _inventory.SetLastItemId(_state.State.LastItemId);
            _streamNamespace = _state.State.StreamNamespace ?? _streamNamespace;
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
            var ids = _inventory.GetItemIds();
            foreach (var id in ids) {
                changes.Add(new ItemChange() { What = ItemChange.Variant.TouchItem, ItemId = id });
            }
            _inventory.Changes = changes;

            await WriteInventoryState(new ItemChangesSummary(_inventory));
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
