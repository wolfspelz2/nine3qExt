using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.GrainInterfaces;
using n3q.Tools;

namespace IntegrationTests
{
    [TestClass]
    public class AzureReflectingTableStorageTest
    {
        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task SetGet()
        {
            // Arrange 
            var grain = GrainClient.GrainFactory.GetGrain<ITestReflecting>($"{nameof(AzureReflectingTableStorageTest)}-{nameof(SetGet)}-{RandomString.Get(10)}");

            try {
                // Act 
                await grain.SetString("Hello Wörld");
                await grain.SetLong(1234567812345678);
                await grain.SetDouble(3.14159265358979);
                await grain.SetBool(true);

                // Assert
                Assert.AreEqual("Hello Wörld", await grain.GetString());
                Assert.AreEqual(1234567812345678, await grain.GetLong());
                Assert.AreEqual(3.14159265358979, await grain.GetDouble());
                Assert.AreEqual(true, await grain.GetBool());

            } finally {
                // Cleanup
                grain.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task ReActivate()
        {
            // Arrange 
            var grain = GrainClient.GrainFactory.GetGrain<ITestReflecting>($"{nameof(AzureReflectingTableStorageTest)}-{nameof(SetGet)}-{RandomString.Get(10)}");

            try {
                await grain.SetString("Hello Wörld");
                await grain.SetLong(1234567812345678);
                await grain.SetDouble(3.14159265358979);
                await grain.SetBool(true);

                // Act 
                await grain.Deactivate();

                // Assert
                Assert.AreEqual("Hello Wörld", await grain.GetString());
                Assert.AreEqual(1234567812345678, await grain.GetLong());
                Assert.AreEqual(3.14159265358979, await grain.GetDouble());
                Assert.AreEqual(true, await grain.GetBool());

            } finally {
                // Cleanup
                grain.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task DeleteStorage()
        {
            // Arrange 
            var grain = GrainClient.GrainFactory.GetGrain<ITestReflecting>($"{nameof(AzureReflectingTableStorageTest)}-{nameof(SetGet)}-{RandomString.Get(10)}");

            try {
                await grain.SetString("Hello Wörld");
                await grain.SetLong(1234567812345678);
                await grain.SetDouble(3.14159265358979);
                await grain.SetBool(true);

                // Act 
                await grain.DeletePersistentStorage();

                // Assert
                Assert.AreEqual(null, await grain.GetString());
                Assert.AreEqual(0L, await grain.GetLong());
                Assert.AreEqual(0.0D, await grain.GetDouble());
                Assert.AreEqual(false, await grain.GetBool());

            } finally {
                // Cleanup
                grain.DeletePersistentStorage().Wait();
            }
        }

    }
}
