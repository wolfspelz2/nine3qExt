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
                    await self.Set(Pid.GreetedAspect, true);
                });
                await greeter.WithTransaction(async self => {
                    await self.Modify(new PropertySet { [Pid.GreeterAspect] = true, [Pid.GreeterPrefix] = "Hello " }, PidSet.Empty);
                });

                // Act
                await GetWorkerGrain().AspectAction(
                    greetedId,
                    Pid.GreetedAspect,
                    nameof(Greeted.GetGreeting),
                    new PropertySet { [Pid.GreetedGetGreetingGreeter] = greeterId, [Pid.GreetedGetGreetingName] = "World" }
                );

                // Assert
                Assert.AreEqual("Hello World", await greeter.GetString(Pid.GreeterResult));
                Assert.AreEqual("Hello World", await greeted.GetString(Pid.GreetedResult));

            } finally {
                // Cleanup
                await greeted.DeletePersistentStorage();
                await greeter.DeletePersistentStorage();
            }
        }

    }
}
