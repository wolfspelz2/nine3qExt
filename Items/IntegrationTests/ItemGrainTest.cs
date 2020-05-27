using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Tools;
using n3q.Items;
using n3q.GrainInterfaces;
using System.Threading;

namespace IntegrationTests
{
    [TestClass]
    public class ItemGrainTest
    {
        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task SetGet_string()
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
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task SetGet_PropertyValue()
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
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties()
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
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties_by_access_level()
        {
            // Arrange
            var item = GrainClient.GrainFactory.GetGrain<IItem>($"{nameof(ItemGrainTest)}-{nameof(GetProperties_by_access_level)}-{RandomString.Get(10)}");

            try {
                await item.Set(Pid.TestInternal, 41);
                await item.Set(Pid.TestPublic, 42);

                // Act
                var props = await item.GetProperties(PidSet.Public);

                // Assert
                Assert.AreEqual(1, props.Count);
                Assert.AreEqual(0, (long)props.Get(Pid.TestInternal));
                Assert.AreEqual(42, (long)props.Get(Pid.TestPublic));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }


        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task Delete()
        {
            // Arrange
            var item = GrainClient.GrainFactory.GetGrain<IItem>($"{nameof(ItemGrainTest)}-{nameof(Delete)}-{RandomString.Get(10)}");

            try {
                await item.Set(Pid.TestInt, 42);
                await item.Set(Pid.TestString, "42");

                // Act
                await item.Delete(Pid.TestInt);

                // Assert
                var props = await item.GetProperties(PidSet.All);
                Assert.AreEqual(1, props.Count);
                Assert.AreEqual("42", (string)props.Get(Pid.TestString));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

    }
}
