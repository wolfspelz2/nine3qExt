using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nine3q.GrainInterfaces;
using nine3q.Tools;

namespace IntegrationTests
{
    [TestClass]
    public class TranslationGrainTest
    {
        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void Get_returns_same_string()
        {
            var cs = GrainClient.GrainFactory.GetGrain<ITranslation>($"{nameof(TranslationGrainTest)}-{nameof(Get_returns_same_string)}-{RandomString.Get(10)}");
            try {
                // Arrange
                var inString = "Hello World";
                cs.Set(inString).Wait();

                // Act
                var outString = cs.Get().Result;

                // Assert
                Assert.AreEqual(inString, outString);

            } finally {
                cs.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void Get_returns_big_string()
        {
            var cs = GrainClient.GrainFactory.GetGrain<ITranslation>($"{nameof(TranslationGrainTest)}-{nameof(Get_returns_big_string)}-{RandomString.Get(10)}");
            try {
                // Arrange
                var inString = new StringBuilder();
                for (int i = 0; i < 10000; i++) {
                    inString.Append("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789");
                }
                cs.Set(inString.ToString()).Wait();

                // Act
                var outString = cs.Get().Result;

                // Assert
                Assert.AreEqual(inString.ToString(), outString);
            } finally {
                cs.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void Just_to_be_sure_check_unicode()
        {
            var cs = GrainClient.GrainFactory.GetGrain<ITranslation>($"{nameof(TranslationGrainTest)}-{nameof(Just_to_be_sure_check_unicode)}-{RandomString.Get(10)}");
            try {
                // Arrange
                var inString = "Hello World äöü 頁首 ハイナー";
                cs.Set(inString).Wait();

                // Act
                var outString = cs.Get().Result;

                // Assert
                Assert.AreEqual(inString, outString);
            } finally {
                cs.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void No_value_returns_null()
        {
            var cs = GrainClient.GrainFactory.GetGrain<ITranslation>($"{nameof(TranslationGrainTest)}-{nameof(No_value_returns_null)}-{RandomString.Get(10)}");
            try {
                // Arrange

                // Act
                var outString = cs.Get().Result;

                // Assert
                Assert.IsNull(outString);
            } finally {
                cs.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void Get_after_unset_returns_null()
        {
            var v = GrainClient.GrainFactory.GetGrain<ITranslation>($"{nameof(TranslationGrainTest)}-{nameof(Get_after_unset_returns_null)}-{RandomString.Get(10)}");
            try {
                // Arrange
                var inString = "Hello World";
                v.Set(inString).Wait();

                // Act
                v.Unset().Wait();
                var outString = v.Get().Result;

                // Assert
                Assert.IsNull(outString);
            } finally {
                v.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void Persists()
        {
            var cs = GrainClient.GrainFactory.GetGrain<ITranslation>($"{nameof(TranslationGrainTest)}-{nameof(Persists)}-{RandomString.Get(10)}");
            try {
                // Arrange
                var aString = "Hello World";
                cs.Set(aString).Wait();

                // Act
                var otherString = "Other String";
                cs.Set(otherString).Wait();
                cs.ReloadPersistentStorage().Wait();
                var outString = cs.Get().Result;

                // Assert
                Assert.AreEqual(otherString, outString);

            } finally {
                cs.DeletePersistentStorage().Wait();
            }
        }
    }
}
