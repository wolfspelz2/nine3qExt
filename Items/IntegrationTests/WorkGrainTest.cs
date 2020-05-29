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
    public class WorkGrainTest
    {
        IWorker GetWorkerGrain() { return GrainClient.GrainFactory.GetGrain<IWorker>(Guid.Empty); }
        IItem GetItemGrain(string id) { return GrainClient.GrainFactory.GetGrain<IItem>(id); }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task Run_Greeter()
        {
            // Arrange
            var greetedId = GrainClient.GetRandomItemId("GREETED");
            var greeterId = GrainClient.GetRandomItemId("GREETER");
            var greeted = GrainClient.GetItemStub(greetedId);
            var greeter = GrainClient.GetItemStub(greeterId);

            try {
                await greeted.WithTransaction(async self => {
                    await self.Set(Pid.TestGreetedAspect, true);
                });
                await greeter.WithTransaction(async self => {
                    await self.ModifyProperties(new PropertySet { [Pid.TestGreeterAspect] = true, [Pid.TestGreeterPrefix] = "Hello " }, PidSet.Empty);
                });

                // Act
                await GetWorkerGrain().Run(greetedId,
                               Pid.TestGreetedAspect,
                               nameof(TestGreeted.Action.UseGreeter),
                               new PropertySet { [Pid.TestGreeted_Item] = greeterId, [Pid.TestGreeted_Name] = "World" });

                // Assert
                Assert.AreEqual("Hello World", await greeter.GetString(Pid.TestGreeter_Result));
                Assert.AreEqual("Hello World", await greeted.GetString(Pid.TestGreeted_Result));

            } finally {
                // Cleanup
                await greeted.DeletePersistentStorage();
                await greeter.DeletePersistentStorage();
            }
        }

    }
}
