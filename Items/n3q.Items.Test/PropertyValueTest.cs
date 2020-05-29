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
            Assert.AreEqual(new ValueList { "42" }.ToString(), ((ValueList)pv).ToString());
            Assert.AreEqual("42", ((ValueList)pv).ToString());
            Assert.AreEqual(new ValueList { "42", "4faaaa09-60a9-413e-8388-2e877e70440d" }.ToString(), ((ValueList)new PropertyValue("42 4faaaa09-60a9-413e-8388-2e877e70440d")).ToString());
            Assert.AreEqual( "42 4faaaa09-60a9-413e-8388-2e877e70440d", ((ValueList)new PropertyValue("42 4faaaa09-60a9-413e-8388-2e877e70440d")).ToString());
        }
    }
}
