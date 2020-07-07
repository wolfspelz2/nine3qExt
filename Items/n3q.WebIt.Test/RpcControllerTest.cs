using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Aspects;
using n3q.Items;
using n3q.WebIt.Controllers;

namespace n3q.WebIt.Test
{
    [TestClass]
    public class RpcControllerTest
    {
        [TestMethod]
        public void GetPayloadHash_returns_correct_hash()
        {
            // Arrange
            var payloadNode = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user1", ["entropy"] = "entropy1", });
            var expect = Tools.Crypto.SHA256Base64("secret" + payloadNode.ToJson());
            var controller = new RpcController(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(), new WebItConfigDefinition { PayloadHashSecret = "secret" }, new SiloSimulator());

            // Act
            var hash = controller.GetPayloadHash(new JsonPath.Dictionary { ["user"] = "user1", ["payload"] = payloadNode, })["result"].String;

            // Assert
            Assert.AreEqual(expect, hash);
        }

        //[TestMethod]
        //public void GetPayloadHash_detects_user_mismatch()
        //{
        //    // Arrange
        //    var payload = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user1", ["entropy"] = "entropy1", }).ToJson(bFormatted: false, bWrapped: false);
        //    var payloadBase64Encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
        //    var expect = Tools.Crypto.SHA256Hex("secret" + payload);
        //    var controller = new RpcController(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(), new WebItConfigDefinition { PayloadHashSecret = "secret" }, new SiloSimulator());

        //    // Act
        //    Assert.ThrowsException<Exception>(() => { _ = controller.ComputePayloadHash(new JsonPath.Dictionary { ["user"] = "wrong user", ["payload"] = payloadBase64Encoded, })["result"].String; });
        //}

        [TestMethod]
        public void GetPayloadHash_detects_missing_arguments()
        {
            // Arrange
            var controller = new RpcController(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(), new WebItConfigDefinition { PayloadHashSecret = "secret" }, new SiloSimulator());

            // Act
            Assert.ThrowsException<Exception>(() => { _ = controller.GetPayloadHash(new JsonPath.Dictionary { ["payload"] = new JsonPath.Node(JsonPath.Node.Type.Dictionary), })["result"].String; });
            Assert.ThrowsException<Exception>(() => { _ = controller.GetPayloadHash(new JsonPath.Dictionary { ["payload"] = "", })["result"].String; });
            Assert.ThrowsException<Exception>(() => { _ = controller.GetPayloadHash(new JsonPath.Dictionary { })["result"].String; });
        }

        [TestMethod]
        public void GetPayloadHash_different_user_different_hash()
        {
            // Arrange
            var payload = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user1", ["entropy"] = "entropy1", }).ToJson(bFormatted: false, bWrapped: false);
            var payloadBase64Encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
            var payload2 = new JsonPath.Node(new Dictionary<string, string> { ["user"] = "user2", ["entropy"] = "entropy1", }).ToJson(bFormatted: false, bWrapped: false);
            var payloadBase64Encoded2 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload2));
            var controller = new RpcController(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(), new WebItConfigDefinition { PayloadHashSecret = "secret" }, new SiloSimulator());

            // Act
            var hash = controller.GetPayloadHash(new JsonPath.Dictionary { ["user"] = "user1", ["payload"] = payloadBase64Encoded, })["result"].String;
            var hash2 = controller.GetPayloadHash(new JsonPath.Dictionary { ["user"] = "user2", ["payload"] = payloadBase64Encoded2, })["result"].String;

            // Assert
            Assert.AreNotEqual(hash, hash2);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task ValidatePartnerToken_validates()
        {
            // Arrange
            var computePayloadHashCntroller = new RpcController(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(), new WebItConfigDefinition { PayloadHashSecret = "secret" }, new SiloSimulator());
            var tokenNode = new JsonPath.Node(JsonPath.Node.Type.Dictionary);
            tokenNode.AsDictionary.Add("api", "https://n3q-api.com/v1");
            var payloadNode = new JsonPath.Node(new Dictionary<string, string> { ["partner"] = "suat-theatre-tf5768gihu89z7t6ftugzuhji97t6fituljnjz6t", ["entropy"] = "entropy1" });
            tokenNode.AsDictionary.Add("payload", payloadNode);
            var payloadJson = payloadNode.ToJson(bFormatted: false, bWrapped: false);
            var hash = computePayloadHashCntroller.ComputePayloadHash(payloadJson);
            tokenNode.AsDictionary.Add("hash", hash);
            var tokenJson = tokenNode.ToJson(bFormatted: false, bWrapped: false);
            var tokenBase64Encoded = Tools.Base64.Encode(tokenJson);

            var controller = new RpcController(
                new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(),
                new WebItConfigDefinition { PayloadHashSecret = "secret" },
                new SiloSimulator() {
                    Items = new Dictionary<string, SiloSimulatorItem> {
                        ["suat-theatre-tf5768gihu89z7t6ftugzuhji97t6fituljnjz6t"] = new SiloSimulatorItem {
                            Properties = new PropertySet {
                                [Pid.PartnerAspect] = true,
                                [Pid.PartnerToken] = tokenBase64Encoded,
                            }
                        }
                    }
                }
            );

            // Act
            // Assert
            await controller.ValidatePartnerToken(tokenBase64Encoded);
            //Assert.ThrowsException<Exception>(async () => { await controller.ValidatePartnerToken("wrong token"); });

            tokenNode.AsDictionary.Add("dummy", true);
            var almostCorrectTokenJson = tokenNode.ToJson(bFormatted: false, bWrapped: false);
            var almostCorrectTokenJsonBase64Encoded = Tools.Base64.Encode(almostCorrectTokenJson);
            await Assert.ThrowsExceptionAsync<Exception>(async () => { await controller.ValidatePartnerToken(almostCorrectTokenJsonBase64Encoded); });

            await Assert.ThrowsExceptionAsync<FormatException>(async () => { await controller.ValidatePartnerToken("plainly wrong token"); });
        }

    }
}
