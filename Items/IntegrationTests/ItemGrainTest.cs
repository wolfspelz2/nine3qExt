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
                var props = await item.GetProperties(PidSet.All);

                // Assert
                Assert.AreEqual(42, (long)props.Get(Pid.TestInt));
                Assert.AreEqual(42, (int)props.Get(Pid.TestInt));
                Assert.AreEqual("42", (string)props.Get(Pid.TestString));

            } finally {
                // Cleanup
                item.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async System.Threading.Tasks.Task GetProperties_by_access_level()
        {
            // Arrange 
            var item = GrainClient.GrainFactory.GetGrain<IItem>($"{nameof(ItemGrainTest)}-{nameof(GetProperties_by_access_level)}-{RandomString.Get(10)}");

            try {
                // Act 
                await item.Set(Pid.TestInternal, 41);
                await item.Set(Pid.TestPublic, 42);
                var props = await item.GetProperties(PidSet.Public);

                // Assert
                Assert.AreEqual(1, props.Count);
                Assert.AreEqual(0, (long)props.Get(Pid.TestInternal));
                Assert.AreEqual(42, (long)props.Get(Pid.TestPublic));

            } finally {
                // Cleanup
                item.DeletePersistentStorage().Wait();
            }
        }

    }
}
