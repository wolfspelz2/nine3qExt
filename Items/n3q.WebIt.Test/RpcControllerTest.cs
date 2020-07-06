using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.WebIt.Controllers;

namespace n3q.WebIt.Test
{
    [TestClass]
    public class RpcControllerTest
    {
        [TestMethod]
        public void ComputePayloadHash_returns_correct_hash()
        {
            // Arrange
            var payload = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user", ["entropy"] = "entropy", }).ToJson(bFormatted: false, bWrapped: false);
            var payloadBase64Encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
            var expect = Tools.Crypto.SHA256Hex("secret" + payload);
            var controller = new RpcController(new WebItConfigDefinition { PayloadHashSecret = "secret" });

            // Act
            var hash = controller.ComputePayloadHash(new JsonPath.Dictionary { ["user"] = "user", ["payload"] = payloadBase64Encoded, })["result"].String;

            // Assert
            Assert.AreEqual(expect, hash);
        }

        //[TestMethod]
        //public void ComputePayloadHash_detects_user_mismatch()
        //{
        //    // Arrange
        //    var payload = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user", ["entropy"] = "entropy", }).ToJson(bFormatted: false, bWrapped: false);
        //    var payloadBase64Encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
        //    var expect = Tools.Crypto.SHA256Hex("secret" + payload);
        //    var controller = new RpcController(new WebItConfigDefinition { PayloadHashSecret = "secret" });

        //    // Act
        //    Assert.ThrowsException<Exception>(() => { _ = controller.ComputePayloadHash(new JsonPath.Dictionary { ["user"] = "wrong user", ["payload"] = payloadBase64Encoded, })["result"].String; });
        //}

        [TestMethod]
        public void ComputePayloadHash_detects_missing_arguments()
        {
            // Arrange
            var payload = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user", ["entropy"] = "entropy", }).ToJson(bFormatted: false, bWrapped: false);
            var payloadBase64Encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
            var expect = Tools.Crypto.SHA256Hex("secret" + payload);
            var controller = new RpcController(new WebItConfigDefinition { PayloadHashSecret = "secret" });

            // Act
            Assert.ThrowsException<Exception>(() => { _ = controller.ComputePayloadHash(new JsonPath.Dictionary { ["payload"] = payloadBase64Encoded, })["result"].String; });
            Assert.ThrowsException<Exception>(() => { _ = controller.ComputePayloadHash(new JsonPath.Dictionary { ["user"] = "", ["payload"] = payloadBase64Encoded, })["result"].String; });
            Assert.ThrowsException<Exception>(() => { _ = controller.ComputePayloadHash(new JsonPath.Dictionary { ["user"] = "user", })["result"].String; });
            Assert.ThrowsException<Exception>(() => { _ = controller.ComputePayloadHash(new JsonPath.Dictionary { ["user"] = "user", ["payload"] = "", })["result"].String; });
            Assert.ThrowsException<Exception>(() => { _ = controller.ComputePayloadHash(new JsonPath.Dictionary { })["result"].String; });
        }

        [TestMethod]
        public void ComputePayloadHash_different_user_different_hash()
        {
            // Arrange
            var payload = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user", ["entropy"] = "entropy", }).ToJson(bFormatted: false, bWrapped: false);
            var payloadBase64Encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
            var payload2 = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user2", ["entropy"] = "entropy", }).ToJson(bFormatted: false, bWrapped: false);
            var payloadBase64Encoded2 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload2));
            var controller = new RpcController(new WebItConfigDefinition { PayloadHashSecret = "secret" });

            // Act
            var hash = controller.ComputePayloadHash(new JsonPath.Dictionary { ["user"] = "user", ["payload"] = payloadBase64Encoded, })["result"].String;
            var hash2 = controller.ComputePayloadHash(new JsonPath.Dictionary { ["user"] = "user2", ["payload"] = payloadBase64Encoded2, })["result"].String;

            // Assert
            Assert.AreNotEqual(hash, hash2);
        }
    }
}
