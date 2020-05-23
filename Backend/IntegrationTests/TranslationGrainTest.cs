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
            var t = GrainClient.GrainFactory.GetGrain<ITranslation>($"{nameof(TranslationGrainTest)}-{nameof(Get_returns_same_string)}-{RandomString.Get(10)}");
            try {
                // Arrange
                var inString = "Hello World";
                t.Set(inString).Wait();

                // Act
                var outString = t.Get().Result;

                // Assert
                Assert.AreEqual(inString, outString);

            } finally {
                t.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void Just_to_be_sure_check_unicode()
        {
            var t = GrainClient.GrainFactory.GetGrain<ITranslation>($"{nameof(TranslationGrainTest)}-{nameof(Just_to_be_sure_check_unicode)}-{RandomString.Get(10)}");
            try {
                // Arrange
                var inString = "Hello World äöü 頁首 ハイナー";
                t.Set(inString).Wait();

                // Act
                var outString = t.Get().Result;

                // Assert
                Assert.AreEqual(inString, outString);
            } finally {
                t.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void No_value_returns_null()
        {
            var t = GrainClient.GrainFactory.GetGrain<ITranslation>($"{nameof(TranslationGrainTest)}-{nameof(No_value_returns_null)}-{RandomString.Get(10)}");
            try {
                // Arrange

                // Act
                var outString = t.Get().Result;

                // Assert
                Assert.IsNull(outString);
            } finally {
                t.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void Persists()
        {
            var t = GrainClient.GrainFactory.GetGrain<ITranslation>($"{nameof(TranslationGrainTest)}-{nameof(Persists)}-{RandomString.Get(10)}");
            try {
                // Arrange
                var aString = "Hello World";
                t.Set(aString).Wait();

                // Act
                var otherString = "Other String";
                t.Set(otherString).Wait();
                t.ReloadPersistentStorage().Wait();
                var outString = t.Get().Result;

                // Assert
                Assert.AreEqual(otherString, outString);

            } finally {
                t.DeletePersistentStorage().Wait();
            }
        }
    }
}
