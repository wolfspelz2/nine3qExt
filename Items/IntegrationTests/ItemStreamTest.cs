using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Tools;
using n3q.Items;
using n3q.GrainInterfaces;
using Orleans.Streams;
using n3q.Common;
using System.Collections.Generic;
using System.Threading;
using Orleans;

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
        IItem GetItemGrain(string id)
        {
            return GrainClient.GrainFactory.GetGrain<IItem>(id);
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

            var itemId = $"{nameof(ItemGrainTest)}-{nameof(ItemUpdate_after_Set)}-{RandomString.Get(10)}";
            var item = GetItemGrain(itemId);
            await item.ModifyProperties(new PropertySet { [Pid.TestInt] = 41 }, PidSet.Empty);

            var handle = await GetItemStream().SubscribeAsync(updateReceiver);
            Task.Run(() => { }).ItemStreamTestPerformAsyncTaskWithoutAwait(t => { exceptions.Add(t.Exception); });

            try {
                // Act
                await item.ModifyProperties(new PropertySet { [Pid.TestInt] = 42 }, PidSet.Empty);
                are.WaitOne(3000);

                // Assert
                var props = await item.GetProperties(new PidSet { Pid.TestInt });
                Assert.AreEqual(42, (long)props[Pid.TestInt]);
                Assert.AreEqual(1, updates.Count);
                Assert.AreEqual(itemId, updates[0].ItemId);
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

            var containerId = $"{nameof(ItemGrainTest)}-{nameof(ItemUpdate_Container_AddChild) + "_CONTAINER"}-{RandomString.Get(10)}";
            var childId = $"{nameof(ItemGrainTest)}-{nameof(ItemUpdate_Container_AddChild) + "_CHILD"}-{RandomString.Get(10)}";
            var container = GetItemGrain(containerId);
            var child = GetItemGrain(childId);
            await container.ModifyProperties(new PropertySet { [Pid.TestInt] = 42 }, PidSet.Empty);
            await child.ModifyProperties(new PropertySet { [Pid.TestInt] = 42 }, PidSet.Empty);

            var handle = await GetItemStream().SubscribeAsync(updateReceiver);
            Task.Run(() => { }).ItemStreamTestPerformAsyncTaskWithoutAwait(t => { exceptions.Add(t.Exception); });

            try {
                // Act
                await container.AddToSet(Pid.Contains, childId);
                await child.ModifyProperties(new PropertySet { [Pid.Container] = containerId }, PidSet.Empty);
                await child.ModifyProperties(new PropertySet { [Pid.TestInt] = 43 }, PidSet.Empty);
                are.WaitOne(3000);

                // Assert
                Assert.AreEqual(0, exceptions.Count);

                var props = await child.GetProperties(new PidSet { Pid.TestInt });
                Assert.AreEqual(43, (long)props[Pid.TestInt]);

                Assert.AreEqual(3, updates.Count);

                Assert.AreEqual(containerId, updates[0].ItemId);
                Assert.AreEqual(Pid.Contains, updates[0].Changes[0].Pid);
                Assert.AreEqual(childId, (string)updates[0].Changes[0].Value);

                Assert.AreEqual(childId, updates[1].ItemId);
                Assert.AreEqual(Pid.Container, updates[1].Changes[0].Pid);
                Assert.AreEqual(containerId, (string)updates[1].Changes[0].Value);

                Assert.AreEqual(childId, updates[2].ItemId);
                Assert.AreEqual(Pid.TestInt, updates[2].Changes[0].Pid);
                Assert.AreEqual(43, (long)updates[2].Changes[0].Value);

            } finally {
                // Cleanup
                await handle?.UnsubscribeAsync();
                handle = null;

                await child.DeletePersistentStorage();
                await container.DeletePersistentStorage();
                are.Close();
            }
        }
    }

    public static class ItemStreamTestAsyncUtilityExtension
    {
        public static void ItemStreamTestPerformAsyncTaskWithoutAwait(this Task task, Action<Task> exceptionHandler)
        {
            var dummy = task?.ContinueWith(t => exceptionHandler(t), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        }
    }

}
