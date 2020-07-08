using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Aspects;
using n3q.Common;
using n3q.Items;
using n3q.Tools;
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
            var payloadNode = new JsonPath.Node(new Dictionary<string, string> { [nameof(Protocol.Rpc.ContextToken.Payload.user)] = "user1", [nameof(Protocol.Rpc.ContextToken.Payload.entropy)] = "entropy1", });
            var expect = Tools.Crypto.SHA256Base64("secret" + payloadNode.ToJson());
            var controller = new RpcController(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(), new WebItConfigDefinition { PayloadHashSecret = "secret" }, null) { ItemClient = new SiloSimulatorClusterClient(new SiloSimulator()) };

            // Act
            var hash = controller.GetPayloadHash(new JsonPath.Dictionary { [nameof(Protocol.Rpc.ContextToken.payload)] = payloadNode, })[nameof(Protocol.Rpc.Response.result)].String;

            // Assert
            Assert.AreEqual(expect, hash);
        }

        [TestMethod]
        public void GetPayloadHash_detects_missing_arguments()
        {
            // Arrange
            var controller = new RpcController(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(), new WebItConfigDefinition { PayloadHashSecret = "secret" }, null) { ItemClient = new SiloSimulatorClusterClient(new SiloSimulator()) };

            // Act
            Assert.ThrowsException<Exception>(() => { _ = controller.GetPayloadHash(new JsonPath.Dictionary { [nameof(Protocol.Rpc.ContextToken.payload)] = new JsonPath.Node(JsonPath.Node.Type.Dictionary), }); });
            Assert.ThrowsException<Exception>(() => { _ = controller.GetPayloadHash(new JsonPath.Dictionary { [nameof(Protocol.Rpc.ContextToken.payload)] = "", }); });
            Assert.ThrowsException<Exception>(() => { _ = controller.GetPayloadHash(new JsonPath.Dictionary { }); });
        }

        [TestMethod]
        public void GetPayloadHash_different_user_different_hash()
        {
            // Arrange
            var payload = new JsonPath.Node(new Dictionary<string, string> { [nameof(Protocol.Rpc.ContextToken.Payload.user)] = "user1", [nameof(Protocol.Rpc.ContextToken.Payload.entropy)] = "entropy1", });
            var payload2 = new JsonPath.Node(new Dictionary<string, string> { [nameof(Protocol.Rpc.ContextToken.Payload.user)] = "user2", [nameof(Protocol.Rpc.ContextToken.Payload.entropy)] = "entropy1", });
            var controller = new RpcController(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(), new WebItConfigDefinition { PayloadHashSecret = "secret" }, null) { ItemClient = new SiloSimulatorClusterClient(new SiloSimulator()) };

            // Act
            var hash = controller.GetPayloadHash(new JsonPath.Dictionary { [nameof(Protocol.Rpc.ContextToken.Payload.user)] = "user1", [nameof(Protocol.Rpc.ContextToken.payload)] = payload, })[nameof(Protocol.Rpc.Response.result)].String;
            var hash2 = controller.GetPayloadHash(new JsonPath.Dictionary { [nameof(Protocol.Rpc.ContextToken.Payload.user)] = "user2", [nameof(Protocol.Rpc.ContextToken.payload)] = payload2, })[nameof(Protocol.Rpc.Response.result)].String;

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
            var payloadNode = new JsonPath.Node(new Dictionary<string, string> { [nameof(Protocol.Rpc.DeveloperToken.Payload.developer)] = "suat-theatre-tf5768gihu89z7t6ftugzuhji97t6fituljnjz6t", [nameof(Protocol.Rpc.DeveloperToken.Payload.entropy)] = "entropy1" });
            tokenNode.AsDictionary.Add(nameof(Protocol.Rpc.DeveloperToken.payload), payloadNode);
            var payloadJson = payloadNode.ToJson(bFormatted: false, bWrapped: false);
            var hash = Aspects.Developer.ComputePayloadHash(payloadHashSecret, payloadJson);
            tokenNode.AsDictionary.Add("hash", hash);
            var tokenJson = tokenNode.ToJson(bFormatted: false, bWrapped: false);
            var tokenBase64Encoded = tokenJson.ToBase64();

            var controller = new RpcController(
                new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(),
                new WebItConfigDefinition { PayloadHashSecret = "secret" },
                null
            ) {
                ItemClient = new SiloSimulatorClusterClient(new SiloSimulator() {
                    Items = new Dictionary<string, SiloSimulatorItem> {
                        ["suat-theatre-tf5768gihu89z7t6ftugzuhji97t6fituljnjz6t"] = new SiloSimulatorItem {
                            Properties = new PropertySet {
                                [Pid.DeveloperAspect] = true,
                                [Pid.DeveloperToken] = tokenBase64Encoded,
                            }
                        }
                    }
                })
            };

            // Act
            // Assert
            await controller.GetDeveloperIdAndValidateTheDeveloperToken(tokenBase64Encoded);
            //Assert.ThrowsException<Exception>(async () => { await controller.ValidatePartnerToken("wrong token"); });

            tokenNode.AsDictionary.Add("dummy", true);
            var almostCorrectTokenJson = tokenNode.ToJson(bFormatted: false, bWrapped: false);
            var almostCorrectTokenJsonBase64Encoded = almostCorrectTokenJson.ToBase64();
            await Assert.ThrowsExceptionAsync<Exception>(async () => { await controller.GetDeveloperIdAndValidateTheDeveloperToken(almostCorrectTokenJsonBase64Encoded); });

            await Assert.ThrowsExceptionAsync<FormatException>(async () => { await controller.GetDeveloperIdAndValidateTheDeveloperToken("plainly wrong token"); });
        }

        [TestMethod]
        public async Task GetItemProperties()
        {
            // Arrange
            var userId = "user1";
            var developerId = "suat1";
            var documentId = "document1";
            var payloadHashSecret = "secret";

            var siloSimulator = SetupSiloSimulator(userId, developerId, documentId);
            var controller = SetupController(payloadHashSecret, siloSimulator);
            var simulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            string developerToken = await GenerateDeveloperToken(developerId, simulatorClient);
            string contextToken = CreateContextToken(userId, documentId, payloadHashSecret);

            // Act
            var pidsNode = new JsonPath.Node(JsonPath.Node.Type.List);
            foreach (var pid in new[] { Pid.Container, Pid.DocumentAspect, Pid.DocumentText, Pid.DocumentMaxLength, Pid.TestPublic, Pid.TestInternal }) {
                pidsNode.AsList.Add(new JsonPath.Node(JsonPath.Node.Type.String, pid.ToString()));
            }
            var response = await controller.GetItemProperties(new JsonPath.Dictionary {
                [nameof(Protocol.Rpc.GetItemPropertiesRequest.developer)] = developerToken,
                [nameof(Protocol.Rpc.GetItemPropertiesRequest.context)] = contextToken,
                [nameof(Protocol.Rpc.GetItemPropertiesRequest.method)] = nameof(RpcController.GetItemProperties),
                [nameof(Protocol.Rpc.GetItemPropertiesRequest.pids)] = pidsNode,
            });

            // Assert
            var resultNode = response[nameof(Protocol.Rpc.Response.result)];
            Assert.AreEqual(userId, (string)resultNode[Pid.Container.ToString()]);
            Assert.AreEqual(true, (bool)resultNode[Pid.DocumentAspect.ToString()]);
            Assert.AreEqual("This is a text", (string)resultNode[Pid.DocumentText.ToString()]);
            Assert.AreEqual(100, (long)resultNode[Pid.DocumentMaxLength.ToString()]);
            Assert.AreEqual(42, (long)resultNode[Pid.TestPublic.ToString()]);
            Assert.AreEqual(0, (long)resultNode[Pid.TestInternal.ToString()]);
        }

        [TestMethod]
        public async Task ExecuteItemAction()
        {
            // Arrange
            var userId = "user1";
            var developerId = "suat1";
            var documentId = "document1";
            var payloadHashSecret = "secret";
            var anotherText = "This is another text.";

            var siloSimulator = SetupSiloSimulator(userId, developerId, documentId);
            var controller = SetupController(payloadHashSecret, siloSimulator);
            var simulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            var developerToken = await GenerateDeveloperToken(developerId, simulatorClient);
            var contextToken = CreateContextToken(userId, documentId, payloadHashSecret);

            // Act
            var response = await controller.ExecuteItemAction(new JsonPath.Dictionary {
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.developer)] = developerToken,
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.context)] = contextToken,
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.method)] = nameof(RpcController.ExecuteItemAction),
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.action)] = nameof(Aspects.Document.Action.SetText),
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.args)] = new JsonPath.Node(new Dictionary<string, string> { ["text"] = anotherText }),
            });

            // Assert
            var documentStub = new ItemWriter(simulatorClient.GetItemClient(documentId), new VoidTransaction());
            Assert.AreEqual(anotherText, await documentStub.GetString(Pid.DocumentText));
        }

        private static RpcController SetupController(string payloadHashSecret, SiloSimulator siloSimulator)
        {
            return new RpcController(
                new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(),
                new WebItConfigDefinition { PayloadHashSecret = payloadHashSecret },
                null
            ) { ItemClient = new SiloSimulatorClusterClient(siloSimulator) };
        }

        private static string CreateContextToken(string userId, string documentId, string payloadHashSecret)
        {
            var contextNode = new JsonPath.Node(JsonPath.Node.Type.Dictionary);
            contextNode.AsDictionary.Add("api", new WebItConfigDefinition().ItemServiceWebApiUrl);
            var payloadNode = new JsonPath.Node(new Dictionary<string, string> {
                [nameof(Protocol.Rpc.ContextToken.Payload.user)] = userId,
                [nameof(Protocol.Rpc.ContextToken.Payload.item)] = documentId,
                ["room"] = "9ca05afb1a49f26fb59642305c481661f8b370bd@muc4.virtual-presence.org",
                [nameof(Protocol.Rpc.ContextToken.Payload.entropy)] = Tools.RandomString.Get(40),
            });
            contextNode.AsDictionary.Add(nameof(Protocol.Rpc.ContextToken.payload), payloadNode);
            contextNode.AsDictionary.Add(nameof(Protocol.Rpc.ContextToken.hash), Aspects.Developer.ComputePayloadHash(payloadHashSecret, payloadNode));
            var contextToken = contextNode.ToJson().ToBase64();
            return contextToken;
        }

        private static async Task<string> GenerateDeveloperToken(string developerId, SiloSimulatorClusterClient simulatorClient)
        {
            var developerStub = new ItemWriter(simulatorClient.GetItemClient(developerId), new VoidTransaction());
            await developerStub.WithoutTransaction(async self => {
                await self.Execute(nameof(Aspects.Developer.Action.GenerateToken), new Dictionary<string, string>());
            });
            var developerToken = (string)await developerStub.Get(Pid.DeveloperToken);
            return developerToken;
        }

        private static SiloSimulator SetupSiloSimulator(string userId, string developerId, string documentId)
        {
            return new SiloSimulator() {
                Items = new Dictionary<string, SiloSimulatorItem> {
                    [Common.ItemService.WebItConfigItemId] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.DocumentAspect] = true,
                            [Pid.DocumentText] = new JsonPath.Node(new Dictionary<string, string> {
                                [Aspects.Developer.PayloadHashSecretConfigName] = new WebItConfigDefinition().PayloadHashSecret,
                                [Aspects.Developer.ItemServiceWebApiUrlConfigName] = new WebItConfigDefinition().ItemServiceWebApiUrl,
                            }).ToJson(),
                        }
                    },
                    [developerId] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.DeveloperAspect] = true,
                        }
                    },
                    [userId] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.ContainerAspect] = true,
                            [Pid.Contains] = new ValueList { documentId },
                        }
                    },
                    [documentId] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.Container] = userId,
                            [Pid.Developer] = developerId,
                            [Pid.DocumentAspect] = true,
                            [Pid.DocumentText] = "This is a text",
                            [Pid.DocumentMaxLength] = 100,
                            [Pid.TestPublic] = 42,
                            [Pid.TestInternal] = 43,
                        }
                    },
                }
            };
        }
    }
}
