using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Tools;
using n3q.Items;
using n3q.GrainInterfaces;

namespace IntegrationTests
{
    [TestClass]
    public class ItemGrainTest
    {
        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async System.Threading.Tasks.Task SetGet_string()
        {
            // Arrange 
            var item = GrainClient.GrainFactory.GetGrain<IItem>($"{nameof(ItemGrainTest)}-{nameof(SetGet_string)}-{RandomString.Get(10)}");

            try {
                // Act 
                await item.Set(Pid.TestString, "42");
                var value = await item.GetString(Pid.TestString);

                // Assert
                Assert.AreEqual("42", value);

            } finally {
                // Cleanup
                item.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async System.Threading.Tasks.Task SetGet_PropertyValue()
        {
            // Arrange 
            var item = GrainClient.GrainFactory.GetGrain<IItem>($"{nameof(ItemGrainTest)}-{nameof(SetGet_PropertyValue)}-{RandomString.Get(10)}");

            try {
                // Act 
                await item.Set(Pid.TestString, "42");
                var value = await item.Get(Pid.TestString);

                // Assert
                Assert.AreEqual("42", (string)value);

            } finally {
                // Cleanup
                item.DeletePersistentStorage().Wait();
            }
        }


        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async System.Threading.Tasks.Task GetProperties()
        {
            // Arrange 
            var item = GrainClient.GrainFactory.GetGrain<IItem>($"{nameof(ItemGrainTest)}-{nameof(GetProperties)}-{RandomString.Get(10)}");

            try {
                // Act 
                await item.Set(Pid.TestInt, 42);
                await item.Set(Pid.TestString, "42");
                var value = await item.Get(Pid.TestString);

                // Assert
                Assert.AreEqual("42", (string)value);

            } finally {
                // Cleanup
                item.DeletePersistentStorage().Wait();
            }
        }
    }
}
