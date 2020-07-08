using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using Orleans.Streams;
using n3q.Common;
using n3q.Tools;
using n3q.Items;
using n3q.GrainInterfaces;
using n3q.Aspects;
using System.Linq;

namespace IntegrationTests
{
    [TestClass]
    public class ItemStreamTest
    {
        private IAsyncStream<ItemUpdate> GetItemStream()
        {
            var streamProvider = GrainClient.GrainFactory.GetStreamProvider(ItemService.StreamProvider);
            var streamId = ItemService.StreamGuid;
            var streamNamespace = ItemService.StreamNamespace;
            var stream = streamProvider.GetStream<ItemUpdate>(streamId, streamNamespace);
            return stream;
        }

        public class UpdateReceiver : IAsyncObserver<ItemUpdate>
        {
            private readonly AutoResetEvent _are;
            private readonly List<ItemUpdate> _updates;

            public UpdateReceiver(AutoResetEvent are, List<ItemUpdate> updates)
            {
                _are = are;
                _updates = updates;
            }

            public Task OnNextAsync(ItemUpdate update, StreamSequenceToken token = null)
            {
                _updates.Add(update);
                _are.Set();
                return Task.CompletedTask;
            }

            public Task OnCompletedAsync() { return Task.CompletedTask; }
            public Task OnErrorAsync(Exception ex) { return Task.CompletedTask; }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task ItemUpdate_after_Set()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var updates = new List<ItemUpdate>();
            var exceptions = new List<Exception>();
            var updateReceiver = new UpdateReceiver(are, updates);

            var itemId = GrainClient.GetRandomItemId();
            var item = GrainClient.GetItemStub(itemId);
            var handle = (StreamSubscriptionHandle<ItemUpdate>)null;

            try {
                await item.WithTransaction(async self => {
                    await item.Modify(new PropertySet { [Pid.TestInt] = 41 }, PidSet.Empty);
                });

                handle = await GetItemStream().SubscribeAsync(updateReceiver);
                Task.Run(() => { }).PerformAsyncTaskWithoutAwait(t => { exceptions.Add(t.Exception); });

                // Act
                await item.WithTransaction(async self => {
                    await item.Modify(new PropertySet { [Pid.TestInt] = 42 }, PidSet.Empty);
                });
                are.WaitOne(3000);

                // Assert
                var props = await item.Get(new PidSet { Pid.TestInt });
                Assert.AreEqual(42, (long)props[Pid.TestInt]);
                Assert.AreEqual(1, updates.Count);
                Assert.AreEqual(itemId, updates[0].ItemId);
                Assert.AreEqual(ItemChange.Mode.SetProperty, updates[0].Changes[0].What);
                Assert.AreEqual(Pid.TestInt, updates[0].Changes[0].Pid);
                Assert.AreEqual(42, (long)updates[0].Changes[0].Value);
                Assert.AreEqual(0, exceptions.Count);

            } finally {
                // Cleanup
                await handle?.UnsubscribeAsync();
                handle = null;

                await item.DeletePersistentStorage();
                are.Close();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task ItemUpdate_Container_AddChild()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var updates = new List<ItemUpdate>();
            var exceptions = new List<Exception>();
            var updateReceiver = new UpdateReceiver(are, updates);

            var containerId = GrainClient.GetRandomItemId("CONTAINER");
            var childId = GrainClient.GetRandomItemId("CHILD");
            var container = GrainClient.GetItemStub(containerId);
            var child = GrainClient.GetItemStub(childId);
            var handle = (StreamSubscriptionHandle<ItemUpdate>)null;

            try {
                await container.WithTransaction(async self => {
                    await self.Modify(new PropertySet { [Pid.ContainerAspect] = true, [Pid.TestInt] = 41 }, PidSet.Empty);
                });
                await child.WithTransaction(async self => {
                    await self.Modify(new PropertySet { [Pid.TestInt] = 42 }, PidSet.Empty);
                });

                handle = await GetItemStream().SubscribeAsync(updateReceiver);
                Task.Run(() => { }).PerformAsyncTaskWithoutAwait(t => { exceptions.Add(t.Exception); });

                // Act
                //await container.AddToList(Pid.Contains, childId);
                //await child.ModifyProperties(new PropertySet { [Pid.Container] = containerId }, PidSet.Empty);
                //await child.ModifyProperties(new PropertySet { [Pid.TestInt] = 43 }, PidSet.Empty);

                await container.WithTransaction(async self => {
                    var localChild = await self.WritableItem(childId);
                    await self.AsContainer().AddChild(localChild);
                    await self.Set(Pid.TestInt, 43);
                    await localChild.Set(Pid.TestInt, 44);
                });

                are.WaitOne(3000);

                // Assert
                Assert.AreEqual(0, exceptions.Count);

                {
                    var props = await container.Get(PidSet.All);
                    Assert.IsTrue(props[Pid.Contains].IsInList(childId));
                    Assert.AreEqual(43, (long)props[Pid.TestInt]);
                }
                {
                    var props = await child.Get(PidSet.All);
                    Assert.AreEqual(containerId, (string)props[Pid.Container]);
                    Assert.AreEqual(44, (long)props[Pid.TestInt]);
                }

                Assert.AreEqual(2, updates.Count);

                Assert.IsNotNull(updates.Where(update => update.ItemId == containerId).SelectMany(update => update.Changes).Where(change => change.What == ItemChange.Mode.AddToList && change.Pid == Pid.Contains && change.Value == childId && change.Length == 1L).FirstOrDefault());
                Assert.IsNotNull(updates.Where(update => update.ItemId == containerId).SelectMany(update => update.Changes).Where(change => change.What == ItemChange.Mode.SetProperty&& change.Pid == Pid.TestInt && change.Value == 43).FirstOrDefault());
                Assert.IsNotNull(updates.Where(update => update.ItemId == childId).SelectMany(update => update.Changes).Where(change => change.What == ItemChange.Mode.SetProperty && change.Pid == Pid.Container && change.Value == containerId).FirstOrDefault());
                Assert.IsNotNull(updates.Where(update => update.ItemId == childId).SelectMany(update => update.Changes).Where(change => change.What == ItemChange.Mode.SetProperty && change.Pid == Pid.TestInt && change.Value == 44).FirstOrDefault());

            } finally {
                // Cleanup
                await handle?.UnsubscribeAsync();
                handle = null;

                await child.DeletePersistentStorage();
                await container.DeletePersistentStorage();
                are.Close();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task ItemUpdate_for_Delete_after_Set()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var updates = new List<ItemUpdate>();
            var exceptions = new List<Exception>();
            var updateReceiver = new UpdateReceiver(are, updates);

            var itemId = GrainClient.GetRandomItemId();
            var item = GrainClient.GetItemStub(itemId);
            var handle = (StreamSubscriptionHandle<ItemUpdate>)null;

            try {
                handle = await GetItemStream().SubscribeAsync(updateReceiver);

                // Act
                await item.WithTransaction(async self => {
                    await item.Modify(new PropertySet { [Pid.TestInt] = 42 }, PidSet.Empty);
                    await item.Delete();
                });

                Task.Run(() => { }).PerformAsyncTaskWithoutAwait(t => { exceptions.Add(t.Exception); });
                are.WaitOne(3000);

                Task.Run(() => { }).PerformAsyncTaskWithoutAwait(t => { exceptions.Add(t.Exception); });
                are.WaitOne(3000);

                // Assert
                Assert.AreEqual(42, await item.GetInt(Pid.TestInt));
                Assert.AreEqual(1, updates.Count);

                Assert.AreEqual(itemId, updates[0].ItemId);
                Assert.AreEqual(ItemChange.Mode.SetProperty, updates[0].Changes[0].What);
                Assert.AreEqual(Pid.TestInt, updates[0].Changes[0].Pid);
                Assert.AreEqual(42, (long)updates[0].Changes[0].Value);

                Assert.AreEqual(itemId, updates[0].ItemId);
                Assert.AreEqual(ItemChange.Mode.DeleteItem, updates[0].Changes[1].What);

                Assert.AreEqual(0, exceptions.Count);

            } finally {
                // Cleanup
                await handle?.UnsubscribeAsync();
                handle = null;

                await item.DeletePersistentStorage();
                are.Close();
            }
        }

    }
}
