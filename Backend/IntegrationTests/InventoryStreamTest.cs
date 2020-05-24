using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Streams;
using nine3q.Tools;
using nine3q.Items;
using nine3q.GrainInterfaces;

namespace IntegrationTests
{
    [TestClass]
    public class InventoryStreamTest : IAsyncObserver<ItemUpdate>
    {
        readonly List<ItemUpdate> _updates = new List<ItemUpdate>();

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task OnNextAsync(ItemUpdate update, StreamSequenceToken token = null)
        {
            _updates.Add(update);
            return Task.CompletedTask;
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task Template_property_change_results_in_item_update_on_template_streamAsync()
        {
            // Arrange
            var uniqueName = $"Test-{nameof(Template_property_change_results_in_item_update_on_template_streamAsync)}-{RandomString.Get(10)}";
            var inventoryName = uniqueName + "-Inventory";
            var tplInventoryName = uniqueName + "-TemplateInventory";
            var templateName = uniqueName + "-Template";
            var templateStreamNamespace = uniqueName + "-TemplateStreamNamespace";

            var tpl = GrainClient.GrainFactory.GetGrain<IInventory>(tplInventoryName);
            await tpl.SetStreamNamespace(templateStreamNamespace);
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(inventoryName);
            await inv.SetTemplateInventoryName(tplInventoryName);

            var streamProvider = GrainClient.GrainFactory.GetStreamProvider(InventoryService.StreamProvider);
            var tplStreamId = await GrainClient.GrainFactory.GetGrain<IInventory>(tplInventoryName).GetStreamId();
            var tplStreamNamespace = await GrainClient.GrainFactory.GetGrain<IInventory>(tplInventoryName).GetStreamNamespace();
            var tplStream = streamProvider.GetStream<ItemUpdate>(tplStreamId, tplStreamNamespace);

            try {
                var tplItemId = await tpl.CreateItem(new PropertySet { [Pid.Name] = templateName, [Pid.TestInt] = (long)41 });
                var itemId = await inv.CreateItem(new PropertySet { [Pid.TemplateName] = templateName });

                await tplStream.SubscribeAsync(this);
                _updates.Clear();

                // Act
                await tpl.SetItemProperties(tplItemId, new PropertySet { [Pid.TestInt] = 42 });
                var props = await inv.GetItemProperties(itemId, PidList.All);
                Assert.AreEqual((long)42, props.GetInt(Pid.TestInt));

                // Assert
                var update = _updates[0];
                Assert.AreEqual(tplInventoryName, update.InventoryId);
                Assert.AreEqual(tplItemId, update.Id);
                Assert.AreEqual(1, update.Pids.Count);
                Assert.AreEqual(Pid.TestInt, update.Pids[0]);

            } finally {
                foreach (var handle in tplStream.GetAllSubscriptionHandles().Result) {
                    await handle.UnsubscribeAsync();
                }
                await inv.DeletePersistentStorage();
                await tpl.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void Template_property_change_results_in_item_update_on_the_template_which_is_using_inventory_stream()
        {
            Template_property_change_results_in_item_update_on_the_template_which_is_using_inventory_stream_Async().Wait();
        }

        public async Task Template_property_change_results_in_item_update_on_the_template_which_is_using_inventory_stream_Async()
        {
            // Arrange
            var uniqueName = $"Test-{nameof(Template_property_change_results_in_item_update_on_the_template_which_is_using_inventory_stream)}-{RandomString.Get(10)}";
            var invInventoryName = uniqueName + "-Inventory";
            var tplInventoryName = uniqueName + "-TemplateInventory";
            var templateName = uniqueName + "-Template";
            var templateStreamNamespace = uniqueName + "-TemplateStreamNamespace";

            var tpl = GrainClient.GrainFactory.GetGrain<IInventory>(tplInventoryName);
            await tpl.SetStreamNamespace(templateStreamNamespace);
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(invInventoryName);
            await inv.SetTemplateInventoryName(tplInventoryName);

            var streamProvider = GrainClient.GrainFactory.GetStreamProvider(InventoryService.StreamProvider);
            var invStreamId = await GrainClient.GrainFactory.GetGrain<IInventory>(invInventoryName).GetStreamId();
            var invStreamNamespace = await GrainClient.GrainFactory.GetGrain<IInventory>(invInventoryName).GetStreamNamespace();
            var invStream = streamProvider.GetStream<ItemUpdate>(invStreamId, invStreamNamespace);

            try {
                var tplItemId = await tpl.CreateItem(new PropertySet { [Pid.Name] = templateName, [Pid.TestInt] = (long)41 });
                var invItemId = await inv.CreateItem(new PropertySet { [Pid.TemplateName] = templateName });

                await invStream.SubscribeAsync(this);
                _updates.Clear();

                // Act
                await tpl.SetItemProperties(tplItemId, new PropertySet { [Pid.TestInt] = 42 });
                var props = await inv.GetItemProperties(invItemId, PidList.All);
                Assert.AreEqual((long)42, props.GetInt(Pid.TestInt));

                // Assert
                var update = _updates[0];
                Assert.AreEqual(invInventoryName, update.InventoryId);
                Assert.AreEqual(invItemId, update.Id);
                Assert.AreEqual(1, update.Pids.Count);
                Assert.AreEqual(Pid.TestInt, update.Pids[0]);

            } finally {
                foreach (var handle in invStream.GetAllSubscriptionHandles().Result) {
                    await handle.UnsubscribeAsync();
                }
                await inv.DeletePersistentStorage();
                await tpl.DeletePersistentStorage();
            }
        }

    }
}
