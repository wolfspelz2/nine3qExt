using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Web.Controllers;

namespace n3q.WebIt.Test
{
    [TestClass]
    public class PayloadHashControllerTest
    {
        [TestMethod]
        public void Returns_correct_hash()
        {
            // Arrange
            var payload = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user", ["entropy"] = "entropy", }).ToJson(bFormatted: false, bWrapped: false);
            var controller = new PayloadHashController(new Web.WebConfigDefinition { PayloadHashSecret = "secret" });
            var payloadBase64Encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
            var expect = Tools.Crypto.SHA256Hex("secret" + payload);

            // Act
            var hash = controller.Get("user", payloadBase64Encoded);

            // Assert
            Assert.AreEqual(expect, hash);
        }

        [TestMethod]
        public void Detects_user_mismatch()
        {
            // Arrange
            var payload = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user", ["entropy"] = "entropy", }).ToJson(bFormatted: false, bWrapped: false);
            var controller = new PayloadHashController(new Web.WebConfigDefinition { PayloadHashSecret = "secret" });
            var payloadBase64Encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
            var expect = Tools.Crypto.SHA256Hex("secret" + payload);

            // Act
            Assert.ThrowsException<Exception>(() => { _ = controller.Get("wrong user", payloadBase64Encoded); });
        }

        [TestMethod]
        public void Detects_missing_arguments()
        {
            // Arrange
            var payload = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user", ["entropy"] = "entropy", }).ToJson(bFormatted: false, bWrapped: false);
            var controller = new PayloadHashController(new Web.WebConfigDefinition { PayloadHashSecret = "secret" });
            var payloadBase64Encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
            var expect = Tools.Crypto.SHA256Hex("secret" + payload);

            // Act
            Assert.ThrowsException<Exception>(() => { _ = controller.Get(null, payloadBase64Encoded); });
            Assert.ThrowsException<Exception>(() => { _ = controller.Get("", payloadBase64Encoded); });
            Assert.ThrowsException<Exception>(() => { _ = controller.Get("user", null); });
            Assert.ThrowsException<Exception>(() => { _ = controller.Get("user", ""); });
        }

        [TestMethod]
        public void Different_user_different_hash()
        {
            // Arrange
            var controller = new PayloadHashController(new Web.WebConfigDefinition { PayloadHashSecret = "secret" });
            var payload1 = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user1", ["entropy"] = "entropy", }).ToJson(bFormatted: false, bWrapped: false);
            var payloadBase64Encoded1 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload1));
            var payload2 = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user2", ["entropy"] = "entropy", }).ToJson(bFormatted: false, bWrapped: false);
            var payloadBase64Encoded2 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload2));

            // Act
            var hash1 = controller.Get("user1", payloadBase64Encoded1);
            var hash2 = controller.Get("user2", payloadBase64Encoded2);

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }
    }
}
