using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Tools;
using n3q.Items;
using n3q.GrainInterfaces;
using n3q.Aspects;

namespace IntegrationTests
{
    [TestClass]
    public class AspectTest
    {
        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task Deletable_Delete()
        {
            // Arrange
            var itemId = GrainClient.GetRandomItemId();
            var item = GrainClient.GetItemStub(itemId);

            try {
                await item.WithTransaction(async self => {
                    await self.Set(Pid.TestInt, 42);
                });
                Assert.AreEqual(42, await item.GetInt(Pid.TestInt));

                // Act
                await item.WithTransaction(async self => {
                    await self.AsDeletable().Delete();
                });

                // Assert
                var itemWithSameItemId = GrainClient.GetItemStub(itemId);
                Assert.AreEqual(0, await itemWithSameItemId.GetInt(Pid.TestInt));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task Deletable_Delete_removes_from_container()
        {
            // Arrange
            var childId = GrainClient.GetRandomItemId();
            var child = GrainClient.GetItemStub(childId);
            var containerId = GrainClient.GetRandomItemId();
            var container= GrainClient.GetItemStub(containerId);

            try {
                await child.WithTransaction(async self => {
                    await self.Set(Pid.TestInt, 42);
                });
                Assert.AreEqual(42, await child.GetInt(Pid.TestInt));
                await container.WithTransaction(async self => {
                    await self.Set(Pid.ContainerAspect, true);
                });
                await container.WithTransaction(async self => {
                    await self.AsContainer().AddChild(await self.Item(childId));
                });
                Assert.AreEqual(containerId, (string)await child.Get(Pid.Container));
                Assert.AreEqual(childId, (string)((ValueList)(await container.Get(Pid.Contains)))[0]);

                // Act
                await child.WithTransaction(async self => {
                    await self.AsDeletable().Delete();
                    //self.MarkForDeletion();
                    //await Task.CompletedTask;
                });

                // Assert
                var childWithSameItemId = GrainClient.GetItemStub(childId);
                Assert.AreEqual("", (string)await childWithSameItemId.Get(Pid.Container));
                Assert.AreEqual("", (string)await container.Get(Pid.Contains));

            } finally {
                // Cleanup
                await child.DeletePersistentStorage();
                await container.DeletePersistentStorage();
            }
        }

    }
}
