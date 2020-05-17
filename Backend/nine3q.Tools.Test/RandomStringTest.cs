using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace nine3q.Tools.Test
{
    [TestClass]
    public class RandomStringTest
    {
        [TestMethod]
        public void RandomString_are_different()
        {
            // Arrange
            // Act
            // Assert
            Assert.IsTrue(RandomString.Get(10) != RandomString.Get(10));
        }

        [TestMethod]
        public void RandomString_desired_legth()
        {
            // Arrange
            // Act
            // Assert
            Assert.AreEqual(10, RandomString.Get(10).Length);
        }
    }
}
