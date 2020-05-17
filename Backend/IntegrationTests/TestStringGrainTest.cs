using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nine3q.GrainInterfaces;
using nine3q.Tools;

namespace IntegrationTests
{
    [TestClass]
    public class TestStringGrainTest
    {
        const int StackFrameNumber = 0;

        [TestMethod]
        public void SetGet()
        {
            // Arrange 
            var testString = GrainClient.GrainFactory.GetGrain<ITestString>($"{nameof(TestStringGrainTest)}-{nameof(SetGet)}-{RandomString.Get(10)}");

            try {
                // Act 
                testString.Set("a").Wait();
                var a = testString.Get().Result;

                // Assert
                Assert.AreEqual("a", a);

            } finally {
                // Cleanup
                testString.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        public void Persist_unload_reload()
        {
            // Arrange 
            var testString = GrainClient.GrainFactory.GetGrain<ITestString>($"{nameof(TestStringGrainTest)}-{nameof(Persist_unload_reload)}-{RandomString.Get(10)}");

            try {
                // Act 
                testString.Set("a").Wait();
                testString.Deactivate();
                var a = testString.Get().Result;

                // Assert
                Assert.AreEqual("a", a);

            } finally {
                // Cleanup
                testString.DeletePersistentStorage().Wait();
            }
        }
    }
}
