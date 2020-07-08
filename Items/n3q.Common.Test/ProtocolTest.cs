using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JsonPath;

namespace n3q.Items.Test
{
    [TestClass]
    public class ProtocolTest
    {
        [TestMethod]
        public void ComputePayloadHash_normalizes_order_and_creates_same_hash()
        {
            // Arrange
            var payloadHashSecret = "secret";

            // Act
            var hash1 = Common.Protocol.ComputePayloadHash(payloadHashSecret, new Dictionary<string, object> {
                ["string1"] = "41",
                ["string2"] = "42",
                ["int"] = 42L,
                ["bool"] = true,
            }.ToJsonNode());

            var hash2 = Common.Protocol.ComputePayloadHash(payloadHashSecret, new Dictionary<string, object> {
                ["string2"] = "42",
                ["string1"] = "41",
                ["int"] = 42L,
                ["bool"] = true,
            }.ToJsonNode());

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [TestMethod]
        public void ComputePayloadHash_normalizes_types_and_creates_same_hash()
        {
            // Arrange
            var payloadHashSecret = "secret";

            // Act
            var hash1 = Common.Protocol.ComputePayloadHash(payloadHashSecret, new Dictionary<string, object> {
                ["string1"] = "41",
                ["string2"] = "42",
                ["int"] = 42L,
                ["bool"] = true,
            }.ToJsonNode());

            var hash2 = Common.Protocol.ComputePayloadHash(payloadHashSecret, new Dictionary<string, object> {
                ["string1"] = "41",
                ["string2"] = 42,
                ["int"] = "42",
                ["bool"] = "true",
            }.ToJsonNode());

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

    }
}
