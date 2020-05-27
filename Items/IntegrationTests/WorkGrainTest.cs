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
        IWorker GetWorkGrain() { return GrainClient.GrainFactory.GetGrain<IWorker>(Guid.Empty); }
        IItem GetItemGrain(string id) { return GrainClient.GrainFactory.GetGrain<IItem>(id); }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task Execute_Greeter()
        {
            // Arrange
            var work = GetWorkGrain();
            var workId = Guid.NewGuid();
            var greetUserId = $"{nameof(WorkGrainTest)}-{nameof(Execute_Greeter) + "_GREETUSER"}-{RandomString.Get(10)}";
            var greeterId = $"{nameof(WorkGrainTest)}-{nameof(Execute_Greeter) + "_GREETER"}-{RandomString.Get(10)}";
            var greetUser = GetItemGrain(greetUserId);
            var greeter = GetItemGrain(greeterId);

            try {
                await greetUser.Set(Pid.TestGreetUserAspect, true);
                await greeter.Set(Pid.TestGreeterAspect, true);
                await greeter.Set(Pid.TestString, "Hello");

                // Act
                var greeting = await work.Run(greetUserId, Pid.TestGreetUserAspect, nameof(TestGreetUser.UseGreeter), new PropertySet { [Pid.Item] = greeterId, [Pid.Name] = "World" });

                // Assert
                Assert.AreEqual("Hello World", (string)greeting);

            } finally {
                // Cleanup
                await greetUser.DeletePersistentStorage();
                await greeter.DeletePersistentStorage();
            }
        }

    }
}
