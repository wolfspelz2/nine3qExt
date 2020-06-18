using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace n3q.Tools.Test
{
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void IsSomething()
        {
            Assert.IsTrue(Has.Value("x"));
            Assert.IsFalse(Has.Value(""));
            Assert.IsFalse(Has.Value((string)null));
            Assert.IsFalse(Has.Value(null));
        }
    }
}
