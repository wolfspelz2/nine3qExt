using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.GrainInterfaces;
using n3q.Tools;

namespace IntegrationTests
{
    [TestClass]
    public class ItemRefGrainTest
    {
        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task Create()
        {
            var itemId = RandomString.Get(50);
            var itemRef = GrainClient.GrainFactory.GetGrain<IItemRef>($"{nameof(ItemRefGrainTest)}-{nameof(Create)}-{RandomString.Get(10)}");
            try {
                // Arrange
                await itemRef.SetItem(itemId);

                // Act
                // Assert
                Assert.AreEqual(itemId, await itemRef.GetItem());

            } finally {
                itemRef.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task PersistsAndReactivates()
        {
            var itemId = RandomString.Get(50);
            var itemRef = GrainClient.GrainFactory.GetGrain<IItemRef>($"{nameof(ItemRefGrainTest)}-{nameof(PersistsAndReactivates)}-{RandomString.Get(10)}");
            try {
                // Arrange
                await itemRef.SetItem(itemId);
                Assert.AreEqual(itemId, await itemRef.GetItem());
                await itemRef.Deactivate();

                // Act
                // Assert
                Assert.AreEqual(itemId, await itemRef.GetItem());

            } finally {
                itemRef.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task NullBeforeSet()
        {
            var itemId = RandomString.Get(50);
            var itemRef = GrainClient.GrainFactory.GetGrain<IItemRef>($"{nameof(ItemRefGrainTest)}-{nameof(NullBeforeSet)}-{RandomString.Get(10)}");
            try {
                // Arrange
                // Act
                // Assert
                Assert.IsNull(await itemRef.GetItem());

            } finally {
                itemRef.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task NullAfterDelete()
        {
            var itemId = RandomString.Get(50);
            var itemRef = GrainClient.GrainFactory.GetGrain<IItemRef>($"{nameof(ItemRefGrainTest)}-{nameof(NullAfterDelete)}-{RandomString.Get(10)}");
            try {
                // Arrange
                await itemRef.SetItem(itemId);
                Assert.AreEqual(itemId, await itemRef.GetItem());

                // Act
                await itemRef.Delete();

                // Assert
                Assert.IsNull(await itemRef.GetItem());

            } finally {
                itemRef.DeletePersistentStorage().Wait();
            }
        }


    }
}
