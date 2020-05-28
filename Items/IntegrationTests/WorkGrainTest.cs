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
        public async Task Run_Greeter()
        {
            // Arrange
            var work = GetWorkGrain();
            var workId = Guid.NewGuid();
            var greetUserId = $"{nameof(WorkGrainTest)}-{nameof(Run_Greeter) + "_GREETUSER"}-{RandomString.Get(10)}";
            var greeterId = $"{nameof(WorkGrainTest)}-{nameof(Run_Greeter) + "_GREETER"}-{RandomString.Get(10)}";
            var greetUser = GetItemGrain(greetUserId);
            var greeter = GetItemGrain(greeterId);

            try {
                await greetUser.ModifyProperties(new PropertySet { [Pid.TestGreetUserAspect] = true }, PidSet.Empty);
                await greeter.ModifyProperties(new PropertySet { [Pid.TestGreeterAspect] = true, [Pid.TestGreeterPrefix] = "Hello " }, PidSet.Empty);

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
