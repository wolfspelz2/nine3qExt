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
    public class WorkerGrainTest
    {
        IWorker GetWorkerGrain() { return GrainClient.GrainFactory.GetGrain<IWorker>(Guid.Empty); }

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
                await GetWorkerGrain().AspectAction(
                    greetedId,
                    Pid.TestGreetedAspect,
                    nameof(TestGreeted.Action.GetGreeting),
                    new PropertySet { [Pid.TestGreetedGetGreetingGreeter] = greeterId, [Pid.TestGreetedGetGreetingName] = "World" }
                );

                // Assert
                Assert.AreEqual("Hello World", await greeter.GetString(Pid.TestGreeterResult));
                Assert.AreEqual("Hello World", await greeted.GetString(Pid.TestGreetedResult));

            } finally {
                // Cleanup
                await greeted.DeletePersistentStorage();
                await greeter.DeletePersistentStorage();
            }
        }

    }
}
