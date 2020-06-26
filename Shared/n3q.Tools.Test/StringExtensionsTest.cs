using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;

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

        [TestMethod]
        public void SimpleHash_spreads()
        {
            var histogram = new Dictionary<int, int>();
            var len = 2;
            var mod = (int)Math.Pow(10, len);
            var count = 100000;
            var average = count / mod;
            for (var i = 0; i < mod; i++) {
                histogram[i] = 0;
            }
            for (var i = 0; i < count; i++) {
                var s = RandomString.Get(RandomInt.Get(1, 30));
                //var s = Guid.NewGuid().ToString();
                var hash = s.SimpleHash();
                var sHash2 = hash % mod;
                histogram[sHash2]++;
            }
            var values = histogram.Values.OrderBy(x => x).ToList();
            var inverted = histogram.Select(pair => new KeyValuePair<int, int>(pair.Value, pair.Key)).OrderBy(kv => kv.Key);
            for (var i = 0; i < mod; i++) {
                Assert.IsTrue(histogram[i] > average * 0.8);
                Assert.IsTrue(histogram[i] < average * 1.2);
            }
        }
    }
}
