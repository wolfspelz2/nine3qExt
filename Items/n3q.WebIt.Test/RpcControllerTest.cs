using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task ValidatePartnerToken_validates()
        {
            // Arrange
            var payloadHashSecret = "secret";
            var tokenNode = new JsonPath.Node(JsonPath.Node.Type.Dictionary);
            tokenNode.AsDictionary.Add("api", "https://n3q-api.com/v1");
            var payloadNode = new JsonPath.Node(new Dictionary<string, string> { ["partner"] = "suat-theatre-tf5768gihu89z7t6ftugzuhji97t6fituljnjz6t", ["entropy"] = "entropy1" });
            tokenNode.AsDictionary.Add("payload", payloadNode);
            var payloadJson = payloadNode.ToJson(bFormatted: false, bWrapped: false);
            var hash = Aspects.Partner.ComputePayloadHash(payloadHashSecret, payloadJson);
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
            await controller.GetPartnerIdAndValidatePartnerToken(tokenBase64Encoded);
            //Assert.ThrowsException<Exception>(async () => { await controller.ValidatePartnerToken("wrong token"); });

            tokenNode.AsDictionary.Add("dummy", true);
            var almostCorrectTokenJson = tokenNode.ToJson(bFormatted: false, bWrapped: false);
            var almostCorrectTokenJsonBase64Encoded = Tools.Base64.Encode(almostCorrectTokenJson);
            await Assert.ThrowsExceptionAsync<Exception>(async () => { await controller.GetPartnerIdAndValidatePartnerToken(almostCorrectTokenJsonBase64Encoded); });

            await Assert.ThrowsExceptionAsync<FormatException>(async () => { await controller.GetPartnerIdAndValidatePartnerToken("plainly wrong token"); });
        }

        [TestMethod]
        public async Task GetItemProperties()
        {
            // Arrange
            var userId = "user1";
            var partnerId = "suat1";
            var documentId = "document1";

            var siloSimulator = new SiloSimulator() {
                Items = new Dictionary<string, SiloSimulatorItem> {
                    [Common.ItemService.WebItConfigItemId] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.DocumentAspect] = true,
                            [Pid.DocumentText] = new JsonPath.Node(new Dictionary<string, string> {
                                [Aspects.Partner.PayloadHashSecretConfigName] = new WebItConfigDefinition().PayloadHashSecret,
                                [Aspects.Partner.ItemServiceWebApiUrlConfigName] = new WebItConfigDefinition().ItemServiceWebApiUrl,
                            }).ToJson(),
                        }
                    },
                    [partnerId] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.PartnerAspect] = true,
                        }
                    },
                    [userId] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.ContainerAspect] = true,
                        }
                    },
                    [documentId] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.Partner] = partnerId,
                            [Pid.DocumentAspect] = true,
                            [Pid.DocumentText] = "This is a text",
                            [Pid.DocumentMaxLength] = 100,
                            [Pid.TestPublic] = 42,
                            [Pid.TestInternal] = 43,
                        }
                    },
                }
            };

            var controller = new RpcController(
                new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(),
                new WebItConfigDefinition { PayloadHashSecret = "secret" },
                siloSimulator
            );

            var simulatorClient = new SiloSimulatorClusterClient(siloSimulator);

            var userStub = new ItemStub(simulatorClient.GetItemClient(userId), new VoidTransaction());
            await userStub.WithoutTransaction(async self => {
                await self.AsContainer().AddChild(await self.Item(documentId));
            });

            var partnerStub = new ItemStub(simulatorClient.GetItemClient(partnerId), new VoidTransaction());
            await partnerStub.WithoutTransaction(async self => {
                await self.Execute(nameof(Aspects.Partner.Action.GenerateToken), new Dictionary<string, string>());
            });
            var partnerToken = (string)await partnerStub.Get(Pid.PartnerToken);

            var contextNode = new JsonPath.Node(JsonPath.Node.Type.Dictionary);
            contextNode.AsDictionary.Add("api", new WebItConfigDefinition().ItemServiceWebApiUrl);
            var payloadNode = new JsonPath.Node(new Dictionary<string, string> {
                ["user"] = userId,
                ["item"] = documentId,
                ["room"] = "9ca05afb1a49f26fb59642305c481661f8b370bd@muc4.virtual-presence.org",
                ["entropy"] = Tools.RandomString.Get(40),
            });
            contextNode.AsDictionary.Add("payload", payloadNode);
            contextNode.AsDictionary.Add("hash", Aspects.Partner.ComputePayloadHash(new WebItConfigDefinition().PayloadHashSecret, payloadNode.ToJson()));
            var contextToken = Tools.Base64.Encode(contextNode.ToJson());

            // Act
            var pidsNode = new JsonPath.Node(JsonPath.Node.Type.List);
            foreach (var pid in new[] { Pid.Container, Pid.DocumentAspect, Pid.DocumentText, Pid.DocumentMaxLength, Pid.TestPublic, Pid.TestInternal }) {
                pidsNode.AsList.Add(new JsonPath.Node(JsonPath.Node.Type.String, pid.ToString()));
            }
            var response = await controller.GetItemProperties(new JsonPath.Dictionary {
                ["partner"] = partnerToken,
                ["context"] = contextToken,
                ["method"] = "getProperties",
                ["pids"] = pidsNode,
            });

            // Assert
            var resultNode = response["result"];
            Assert.AreEqual(userId, (string)resultNode[Pid.Container.ToString()]);
            Assert.AreEqual(true, (bool)resultNode[Pid.DocumentAspect.ToString()]);
            Assert.AreEqual("This is a text", (string)resultNode[Pid.DocumentText.ToString()]);
            Assert.AreEqual(100, (long)resultNode[Pid.DocumentMaxLength.ToString()]);
            Assert.AreEqual(42, (long)resultNode[Pid.TestPublic.ToString()]);
            Assert.AreEqual(0, (long)resultNode[Pid.TestInternal.ToString()]);
        }

    }
}
