using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace n3q.Tools.Test
{
    [TestClass]
    public class LongDateTimeTest
    {
        [TestMethod]
        public void ToLong()
        {
            // Unixtime 1000000000
            Assert.AreEqual(new DateTime(2001, 09, 09, 01, 46, 40).ToLong(), LongDateTime.Epoch + 20010909014640000);

            // Unixtime 1234567890
            Assert.AreEqual(new DateTime(2009, 02, 13, 23, 31, 30, 123).ToLong(), LongDateTime.Epoch + 20090213233130123);

            Assert.AreEqual(new DateTime(1900, 1, 1, 0, 0, 0, 1).ToLong(), LongDateTime.Epoch + 19000101000000001);
            Assert.AreEqual(new DateTime(9999, 1, 1, 1, 1, 1, 1).ToLong(), LongDateTime.Epoch + 99990101010101001);

            // 0 = 1.1.1900 00:00:00 0ms
            Assert.AreEqual(DateTime.MinValue.ToLong(), 0L);
        }

        [TestMethod]
        public void FromLong()
        {
            // Unixtime 1000000000
            Assert.AreEqual(new DateTime(2001, 09, 09, 01, 46, 40), new DateTime().FromLong(LongDateTime.Epoch + 20010909014640000));

            // Unixtime 1234567890
            Assert.AreEqual(new DateTime(2009, 02, 13, 23, 31, 30, 123), new DateTime().FromLong(LongDateTime.Epoch + 20090213233130123));

            Assert.AreEqual(new DateTime(1900, 1, 1, 0, 0, 0, 1), new DateTime().FromLong(LongDateTime.Epoch + 19000101000000001));
            Assert.AreEqual(new DateTime(9999, 1, 1, 1, 1, 1, 1), new DateTime().FromLong(LongDateTime.Epoch + 99990101010101001));

            // 0 = 1.1.1900 00:00:00 0ms
            Assert.AreEqual(DateTime.MinValue, new DateTime().FromLong(LongDateTime.Epoch + 0));
            Assert.AreEqual(DateTime.MinValue, new DateTime().FromLong(0L));
        }

        [TestMethod]
        public void Encode_decode_returns_input()
        {
            // Unixtime 1000000000
            Assert.AreEqual(new DateTime(2001, 09, 09, 01, 46, 40), new DateTime().FromLong(new DateTime(2001, 09, 09, 01, 46, 40).ToLong()));

            // Unixtime 1234567890
            Assert.AreEqual(new DateTime(2009, 02, 13, 23, 31, 30, 123), new DateTime().FromLong(new DateTime(2009, 02, 13, 23, 31, 30, 123).ToLong()));

            Assert.AreEqual(new DateTime(1900, 1, 1, 0, 0, 0, 1), new DateTime().FromLong(new DateTime(1900, 1, 1, 0, 0, 0, 1).ToLong()));
            Assert.AreEqual(new DateTime(9999, 1, 1, 1, 1, 1, 1), new DateTime().FromLong(new DateTime(9999, 1, 1, 1, 1, 1, 1).ToLong()));
            Assert.AreEqual(new DateTime(1601, 1, 1, 0, 0, 0, 1), new DateTime().FromLong(new DateTime(1601, 1, 1, 0, 0, 0, 1).ToLong()));
            Assert.AreEqual(new DateTime(1753, 1, 1, 0, 0, 0, 1), new DateTime().FromLong(new DateTime(1753, 1, 1, 0, 0, 0, 1).ToLong()));
            Assert.AreEqual(new DateTime(0001, 1, 1, 0, 0, 0, 1), new DateTime().FromLong(new DateTime(0001, 1, 1, 0, 0, 0, 1).ToLong()));
        }

    }
}
