using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace nine3q.Items.Test
{
    [TestClass]
    public class PropertyValueTest
    {
        [TestMethod]
        public void Implicit_operators()
        {
            // Arrange
            var pv = new PropertyValue("42");

            // Act
            string s = pv;

            // Assert
            Assert.AreEqual("42", s);
            Assert.AreEqual("42", (string)pv);
            Assert.AreEqual((long)42, (long)pv);
        }

    }
}
