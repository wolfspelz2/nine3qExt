using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.GrainInterfaces;
using n3q.Tools;

namespace IntegrationTests
{
    [TestClass]
    public class CachedStringGrainTest
    {
        [TestMethod][TestCategory(GrainClient.Category)]
        public void Get_returns_same_string()
        {
            // Arrange
            var inString = "Hello World";
            var cs = GrainClient.GrainFactory.GetGrain<ICachedString>($"{nameof(CachedStringGrainTest)}-{nameof(Get_returns_same_string)}-{RandomString.Get(10)}");
            cs.Set(inString).Wait();

            // Act
            var outString = cs.Get().Result;

            // Assert
            Assert.AreEqual(inString, outString);
        }

        [TestMethod][TestCategory(GrainClient.Category)]
        public void Get_returns_big_string()
        {
            // Arrange
            var inString = new StringBuilder();
            for (int i = 0; i < 10000; i++) {
                inString.Append("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789");
            }
            var cs = GrainClient.GrainFactory.GetGrain<ICachedString>($"{nameof(CachedStringGrainTest)}-{nameof(Get_returns_big_string)}-{RandomString.Get(10)}");
            cs.Set(inString.ToString()).Wait();

            // Act
            var outString = cs.Get().Result;

            // Assert
            Assert.AreEqual(inString.ToString(), outString);
        }

        [TestMethod][TestCategory(GrainClient.Category)]
        public void No_value_returns_null()
        {
            // Arrange
            var cs = GrainClient.GrainFactory.GetGrain<ICachedString>($"{nameof(CachedStringGrainTest)}-{nameof(No_value_returns_null)}-{RandomString.Get(10)}");

            // Act
            var outString = cs.Get().Result;

            // Assert
            Assert.IsNull(outString);
        }

        [TestMethod][TestCategory(GrainClient.Category)]
        public void String_expires_when_too_late()
        {
            // Arrange
            var inString = "Hello World";
            var cs = GrainClient.GrainFactory.GetGrain<ICachedString>($"{nameof(CachedStringGrainTest)}-{nameof(String_expires_when_too_late)}-{RandomString.Get(10)}");
            var time = DateTime.UtcNow;
            cs.SetTime(time).Wait();
            cs.Set(inString, 3600).Wait();

            // Act
            cs.SetTime(time.AddSeconds(7200)).Wait();
            var outString = cs.Get().Result;

            // Assert
            Assert.IsNull(outString);
        }

        [TestMethod][TestCategory(GrainClient.Category)]
        public void String_not_expired_when_early_enough()
        {
            // Arrange
            var inString = "Hello World";
            var cs = GrainClient.GrainFactory.GetGrain<ICachedString>($"{nameof(CachedStringGrainTest)}-{nameof(String_not_expired_when_early_enough)}-{RandomString.Get(10)}");
            var time = DateTime.UtcNow;
            cs.SetTime(time).Wait();
            cs.Set(inString, 3600).Wait();

            // Act
            cs.SetTime(time.AddSeconds(1800)).Wait();
            var outString = cs.Get().Result;

            // Assert
            Assert.AreEqual(inString, outString);
        }

        [TestMethod][TestCategory(GrainClient.Category)]
        public void Get_after_unset_returns_null()
        {
            // Arrange
            var inString = "Hello World";
            var cs = GrainClient.GrainFactory.GetGrain<ICachedString>($"{nameof(CachedStringGrainTest)}-{nameof(Get_after_unset_returns_null)}-{RandomString.Get(10)}");
            cs.Set(inString).Wait();

            // Act
            cs.Unset();
            var outString = cs.Get().Result;

            // Assert
            Assert.IsNull(outString);
        }
    }
}
