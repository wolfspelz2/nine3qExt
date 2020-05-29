using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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
            Assert.AreEqual(42L, (long)pv);

            // More
            Assert.AreEqual(new ItemIdList { "42" }.ToString(), ((ItemIdList)pv).ToString());
            Assert.AreEqual("42", ((ItemIdList)pv).ToString());
            Assert.AreEqual(new ItemIdList { "42", "4faaaa09-60a9-413e-8388-2e877e70440d" }.ToString(), ((ItemIdList)new PropertyValue("42 4faaaa09-60a9-413e-8388-2e877e70440d")).ToString());
            Assert.AreEqual( "42 4faaaa09-60a9-413e-8388-2e877e70440d", ((ItemIdList)new PropertyValue("42 4faaaa09-60a9-413e-8388-2e877e70440d")).ToString());
        }
    }
}
