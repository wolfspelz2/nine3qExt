using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace n3q.Items.Test
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

            // More
            Assert.AreEqual(new ItemIdSet { "42" }.ToString(), ((ItemIdSet)pv).ToString());
            Assert.AreEqual("42", ((ItemIdSet)pv).ToString());
            Assert.AreEqual(new ItemIdSet { "42", "4faaaa09-60a9-413e-8388-2e877e70440d" }.ToString(), ((ItemIdSet)new PropertyValue("42 4faaaa09-60a9-413e-8388-2e877e70440d")).ToString());
            Assert.AreEqual( "42 4faaaa09-60a9-413e-8388-2e877e70440d", ((ItemIdSet)new PropertyValue("42 4faaaa09-60a9-413e-8388-2e877e70440d")).ToString());
        }

    }
}
