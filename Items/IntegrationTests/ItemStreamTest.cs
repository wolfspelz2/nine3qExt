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

namespace IntegrationTests
{
    [TestClass]
    public class ItemStreamTest
    {
        private IAsyncStream<ItemUpdate> ItemUpdateStream
        {
            get {
                var streamProvider = GrainClient.GrainFactory.GetStreamProvider(ItemService.StreamProvider);
                var streamId = ItemService.StreamGuid;
                var streamNamespace = ItemService.StreamNamespace;
                var stream = streamProvider.GetStream<ItemUpdate>(streamId, streamNamespace);
                return stream;
            }
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
            var itemId = $"{nameof(ItemGrainTest)}-{nameof(ItemUpdate_after_Set)}-{RandomString.Get(10)}";
            var item = GrainClient.GrainFactory.GetGrain<IItem>(itemId);
            var are = new AutoResetEvent(false);
            await item.Set(Pid.TestInt, 41);
            var updates = new List<ItemUpdate>();
            var exceptions = new List<Exception>();
            var updateReceiver = new UpdateReceiver(are, updates);
            var handle = await ItemUpdateStream.SubscribeAsync(updateReceiver);
            Task.Run(() => {
                //throw new Exception("in Task.Run");
            }).ItemStreamTestPerformAsyncTaskWithoutAwait(t => {
                exceptions.Add(t.Exception);
            });

            try {
                // Act
                await item.Set(Pid.TestInt, 42);
                are.WaitOne(3000);

                // Assert
                Assert.AreEqual(42, (long)await item.GetInt(Pid.TestInt));
                Assert.AreEqual(1, updates.Count);
                Assert.AreEqual(itemId, updates[0].ItemId);
                Assert.AreEqual(Pid.TestInt, updates[0].Pid);
                Assert.AreEqual(42, (long)updates[0].Value);
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
