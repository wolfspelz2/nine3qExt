using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JsonPath;
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
        const string VARIANT = "2";
        const string USERID = "user1";
        const string USERID2 = "user2";
        const string DEVELOPERID = "developer1";
        const string DOCUMENTID = "document1";
        const string DOCUMENTID2 = "document2";
        const string DOCUMENTTEXT = "This is a text";
        const string PAYLOADHASHSECRET = "secret";
        const string ANOTHERTEXT = "This is another text.";
        const string API = "https://n3q-api.com/v1";

        [TestMethod]
        public void GetPayloadHash_returns_correct_hash()
        {
            // Arrange
            var payloadNode = new JsonPath.Node(new Dictionary<string, string> { [nameof(Protocol.ContextToken.Payload.user)] = USERID, [nameof(Protocol.ContextToken.Payload.entropy)] = "entropy1", });
            var expect = Tools.Crypto.SHA256Base64("secret" + payloadNode.Normalized().ToJson());
            var controller = new RpcController(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(), new WebItConfigDefinition { PayloadHashSecret = PAYLOADHASHSECRET }, null) { ClusterClient = new SiloSimulatorClusterClient(new SiloSimulator()) };

            // Act
            var hash = controller.GetPayloadHash(new JsonPath.Dictionary { [nameof(Protocol.ContextToken.payload)] = payloadNode, })[nameof(Protocol.Rpc.Response.result)].String;

            // Assert
            Assert.AreEqual(expect, hash);
        }

        [TestMethod]
        public void GetPayloadHash_detects_missing_arguments()
        {
            // Arrange
            var controller = new RpcController(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(), new WebItConfigDefinition { PayloadHashSecret = PAYLOADHASHSECRET }, null) { ClusterClient = new SiloSimulatorClusterClient(new SiloSimulator()) };

            // Act
            Assert.ThrowsException<Exception>(() => { _ = controller.GetPayloadHash(new JsonPath.Dictionary { [nameof(Protocol.ContextToken.payload)] = new JsonPath.Node(JsonPath.Node.Type.Dictionary), }); });
            Assert.ThrowsException<Exception>(() => { _ = controller.GetPayloadHash(new JsonPath.Dictionary { [nameof(Protocol.ContextToken.payload)] = "", }); });
            Assert.ThrowsException<Exception>(() => { _ = controller.GetPayloadHash(new JsonPath.Dictionary { }); });
        }

        [TestMethod]
        public void GetPayloadHash_different_user_different_hash()
        {
            // Arrange
            var payload = new JsonPath.Node(new Dictionary<string, string> { [nameof(Protocol.ContextToken.Payload.user)] = USERID, [nameof(Protocol.ContextToken.Payload.entropy)] = "entropy1", });
            var payload2 = new JsonPath.Node(new Dictionary<string, string> { [nameof(Protocol.ContextToken.Payload.user)] = USERID2, [nameof(Protocol.ContextToken.Payload.entropy)] = "entropy1", });
            var controller = new RpcController(new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(), new WebItConfigDefinition { PayloadHashSecret = PAYLOADHASHSECRET }, null) { ClusterClient = new SiloSimulatorClusterClient(new SiloSimulator()) };

            // Act
            var hash = controller.GetPayloadHash(new JsonPath.Dictionary { [nameof(Protocol.ContextToken.Payload.user)] = USERID, [nameof(Protocol.ContextToken.payload)] = payload, })[nameof(Protocol.Rpc.Response.result)].String;
            var hash2 = controller.GetPayloadHash(new JsonPath.Dictionary { [nameof(Protocol.ContextToken.Payload.user)] = USERID2, [nameof(Protocol.ContextToken.payload)] = payload2, })[nameof(Protocol.Rpc.Response.result)].String;

            // Assert
            Assert.AreNotEqual(hash, hash2);
        }

        [TestMethod]
        public async Task ValidatePartnerToken_validates()
        {
            // Arrange
            var tokenNode = new JsonPath.Node(JsonPath.Node.Type.Dictionary);
            tokenNode.AsDictionary.Add("api", API);
            var payloadNode = new JsonPath.Node(new Dictionary<string, string> { [nameof(Protocol.DeveloperToken.Payload.developer)] = "suat-theatre-tf5768gihu89z7t6ftugzuhji97t6fituljnjz6t", [nameof(Protocol.DeveloperToken.Payload.entropy)] = "entropy1" });
            tokenNode.AsDictionary.Add(nameof(Protocol.DeveloperToken.payload), payloadNode);
            var payloadJson = payloadNode.ToJson(bFormatted: false, bWrapped: false);
            var hash = Common.Protocol.ComputePayloadHash(PAYLOADHASHSECRET, payloadJson);
            tokenNode.AsDictionary.Add("hash", hash);
            var tokenJson = tokenNode.ToJson(bFormatted: false, bWrapped: false);
            var tokenBase64Encoded = tokenJson.ToBase64();

            var controller = new RpcController(
                new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(),
                new WebItConfigDefinition { PayloadHashSecret = "secret" },
                null
            ) {
                ClusterClient = new SiloSimulatorClusterClient(new SiloSimulator() {
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
            var siloSimulator = SetupSiloSimulator(VARIANT, USERID, DEVELOPERID, DOCUMENTID, DOCUMENTTEXT, 100);
            var controller = SetupController(PAYLOADHASHSECRET, siloSimulator);
            var simulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            string developerToken = await GenerateDeveloperToken(DEVELOPERID, simulatorClient);
            string contextToken = CreateContextToken(USERID, DOCUMENTID, PAYLOADHASHSECRET);

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
            Assert.AreEqual(USERID, (string)resultNode[Pid.Container.ToString()]);
            Assert.AreEqual(true, (bool)resultNode[Pid.DocumentAspect.ToString()]);
            Assert.AreEqual("This is a text", (string)resultNode[Pid.DocumentText.ToString()]);
            Assert.AreEqual(100, (long)resultNode[Pid.DocumentMaxLength.ToString()]);
            Assert.AreEqual(42, (long)resultNode[Pid.TestPublic.ToString()]);
            Assert.AreEqual(0, (long)resultNode[Pid.TestInternal.ToString()]);
        }

        [TestMethod]
        public async Task GetItemProperties_capitalizes_property_names()
        {
            // Arrange
            var siloSimulator = SetupSiloSimulator(VARIANT, USERID, DEVELOPERID, DOCUMENTID, DOCUMENTTEXT, 100);
            var controller = SetupController(PAYLOADHASHSECRET, siloSimulator);
            var simulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            string developerToken = await GenerateDeveloperToken(DEVELOPERID, simulatorClient);
            string contextToken = CreateContextToken(USERID, DOCUMENTID, PAYLOADHASHSECRET);

            // Act
            var pidsNode = new JsonPath.Node(JsonPath.Node.Type.List);
            foreach (var pid in new[] { Pid.Container, Pid.DocumentAspect, Pid.DocumentText, Pid.DocumentMaxLength, Pid.TestPublic, Pid.TestInternal }) {
                pidsNode.AsList.Add(new JsonPath.Node(JsonPath.Node.Type.String, pid.ToString().Substring(0, 1).ToLower() + pid.ToString().Substring(1)));
            }
            var response = await controller.GetItemProperties(new JsonPath.Dictionary {
                [nameof(Protocol.Rpc.GetItemPropertiesRequest.developer)] = developerToken,
                [nameof(Protocol.Rpc.GetItemPropertiesRequest.context)] = contextToken,
                [nameof(Protocol.Rpc.GetItemPropertiesRequest.method)] = nameof(RpcController.GetItemProperties),
                [nameof(Protocol.Rpc.GetItemPropertiesRequest.pids)] = pidsNode,
            });

            // Assert
            var resultNode = response[nameof(Protocol.Rpc.Response.result)];
            Assert.AreEqual(USERID, (string)resultNode[Pid.Container.ToString()]);
            Assert.AreEqual(true, (bool)resultNode[Pid.DocumentAspect.ToString()]);
            Assert.AreEqual("This is a text", (string)resultNode[Pid.DocumentText.ToString()]);
            Assert.AreEqual(100, (long)resultNode[Pid.DocumentMaxLength.ToString()]);
            Assert.AreEqual(42, (long)resultNode[Pid.TestPublic.ToString()]);
            Assert.AreEqual(0, (long)resultNode[Pid.TestInternal.ToString()]);
        }

        [TestMethod]
        public async Task ExecuteItemAction_Document_SetText()
        {
            // Arrange
            var siloSimulator = SetupSiloSimulator(VARIANT, USERID, DEVELOPERID, DOCUMENTID, DOCUMENTTEXT, 100);
            var controller = SetupController(PAYLOADHASHSECRET, siloSimulator);
            var simulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            var developerToken = await GenerateDeveloperToken(DEVELOPERID, simulatorClient);
            var contextToken = CreateContextToken(USERID, DOCUMENTID, PAYLOADHASHSECRET);

            // Act
            var response = await controller.ExecuteItemAction(new JsonPath.Dictionary {
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.developer)] = developerToken,
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.context)] = contextToken,
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.method)] = nameof(RpcController.ExecuteItemAction),
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.action)] = nameof(Aspects.Document.SetText),
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.args)] = new JsonPath.Node(new Dictionary<string, string> { ["text"] = ANOTHERTEXT }),
            });

            // Assert
            var documentStub = simulatorClient.GetItemReader(DOCUMENTID);
            Assert.AreEqual(ANOTHERTEXT, await documentStub.GetString(Pid.DocumentText));
        }

        [TestMethod]
        public async Task ExecuteItemAction_Document_SetText_capitalizes_action()
        {
            // Arrange
            var anotherText = "This is another text.";

            var siloSimulator = SetupSiloSimulator(VARIANT, USERID, DEVELOPERID, DOCUMENTID, DOCUMENTTEXT, 100);
            var controller = SetupController(PAYLOADHASHSECRET, siloSimulator);
            var simulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            var developerToken = await GenerateDeveloperToken(DEVELOPERID, simulatorClient);
            var contextToken = CreateContextToken(USERID, DOCUMENTID, PAYLOADHASHSECRET);

            // Act
            var response = await controller.ExecuteItemAction(new JsonPath.Dictionary {
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.developer)] = developerToken,
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.context)] = contextToken,
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.method)] = nameof(RpcController.ExecuteItemAction).CamelCase(),
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.action)] = nameof(Aspects.Document.SetText).CamelCase(),
                [nameof(Protocol.Rpc.ExecuteItemActionRequest.args)] = new JsonPath.Node(new Dictionary<string, string> { ["text"] = anotherText }),
            });

            // Assert
            var documentStub = simulatorClient.GetItemReader(DOCUMENTID);
            Assert.AreEqual(anotherText, await documentStub.GetString(Pid.DocumentText));
        }

        [TestMethod]
        public async Task ExecuteItemAction_Document_SetText_DocumentMaxSize_exceeded()
        {
            // Arrange
            var aLongText = "1234567 1 1234567 2 1234567 3 1234567 4 1234567 5 1234567 6 1234567 7 1234567 8 1234567 9 123456 10 123456 11 ";

            var siloSimulator = SetupSiloSimulator(VARIANT, USERID, DEVELOPERID, DOCUMENTID, DOCUMENTTEXT, 100);
            var controller = SetupController(PAYLOADHASHSECRET, siloSimulator);
            var simulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            var developerToken = await GenerateDeveloperToken(DEVELOPERID, simulatorClient);
            var contextToken = CreateContextToken(USERID, DOCUMENTID, PAYLOADHASHSECRET);

            // Act
            await Assert.ThrowsExceptionAsync<Exception>(async () => {
                var response = await controller.ExecuteItemAction(new JsonPath.Dictionary {
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.developer)] = developerToken,
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.context)] = contextToken,
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.method)] = nameof(RpcController.ExecuteItemAction),
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.action)] = nameof(Aspects.Document.SetText),
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.args)] = new JsonPath.Node(new Dictionary<string, string> { ["text"] = aLongText }),
                });
            });

            // Assert
            var documentStub = simulatorClient.GetItemReader(DOCUMENTID);
            Assert.AreEqual(DOCUMENTTEXT, await documentStub.GetString(Pid.DocumentText));
        }

        [TestMethod]
        public async Task ExecuteItemAction_Document_SetText_manipulated_context_token_detects_hash_mismatch()
        {
            // Arrange
            var anotherText = "This is another text.";

            var siloSimulator = SetupSiloSimulator(VARIANT, USERID, DEVELOPERID, DOCUMENTID2, DOCUMENTTEXT, 100);
            var controller = SetupController(PAYLOADHASHSECRET, siloSimulator);
            var simulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            var developerToken = await GenerateDeveloperToken(DEVELOPERID, simulatorClient);
            var contextToken = CreateContextToken(USERID, DOCUMENTID, PAYLOADHASHSECRET);

            // change context token itemId
            var contextTokenNode = contextToken.FromBase64().ToJsonNode();
            contextTokenNode[nameof(Protocol.ContextToken.payload)][nameof(Protocol.ContextToken.Payload.item)] = DOCUMENTID2;
            contextToken = contextTokenNode.ToJson().ToBase64();

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () => {
                var response = await controller.ExecuteItemAction(new JsonPath.Dictionary {
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.developer)] = developerToken,
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.context)] = contextToken,
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.method)] = nameof(RpcController.ExecuteItemAction),
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.action)] = nameof(Aspects.Document.SetText),
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.args)] = new JsonPath.Node(new Dictionary<string, string> { ["text"] = anotherText }),
                });
            });
        }

        [TestMethod]
        public async Task ExecuteItemAction_Document_SetText_manipulated_context_token_detects_developer_mismatch()
        {
            // Arrange
            var anotherText = "This is another text.";

            var siloSimulator = SetupSiloSimulator(VARIANT, USERID, DEVELOPERID, DOCUMENTID, DOCUMENTTEXT, 100);
            var controller = SetupController(PAYLOADHASHSECRET, siloSimulator);
            var simulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            var developerToken = await GenerateDeveloperToken(DEVELOPERID + VARIANT, simulatorClient);
            var contextToken = CreateContextToken(USERID, DOCUMENTID, PAYLOADHASHSECRET);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () => {
                var response = await controller.ExecuteItemAction(new JsonPath.Dictionary {
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.developer)] = developerToken,
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.context)] = contextToken,
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.method)] = nameof(RpcController.ExecuteItemAction),
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.action)] = nameof(Aspects.Document.SetText),
                    [nameof(Protocol.Rpc.ExecuteItemActionRequest.args)] = new JsonPath.Node(new Dictionary<string, string> { ["text"] = anotherText }),
                });
            });
        }

        #region Setup

        private static SiloSimulator SetupSiloSimulator(string variant, string userId, string developerId, string documentId, string documentText, long maxDocumentLength)
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
                    [developerId + variant] = new SiloSimulatorItem {
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
                            [Pid.DocumentText] = documentText,
                            [Pid.DocumentMaxLength] = maxDocumentLength,
                            [Pid.TestPublic] = 42,
                            [Pid.TestInternal] = 43,
                        }
                    },
                }
            };
        }

        private static RpcController SetupController(string payloadHashSecret, SiloSimulator siloSimulator)
        {
            return new RpcController(
                new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory().CreateLogger<RpcController>(),
                new WebItConfigDefinition { PayloadHashSecret = payloadHashSecret },
                null
            ) { ClusterClient = new SiloSimulatorClusterClient(siloSimulator) };
        }

        private static string CreateContextToken(string userId, string documentId, string payloadHashSecret)
        {
            var contextNode = new JsonPath.Node(JsonPath.Node.Type.Dictionary);
            contextNode.AsDictionary.Add("api", new WebItConfigDefinition().ItemServiceWebApiUrl);
            var payloadNode = new JsonPath.Node(new Dictionary<string, string> {
                [nameof(Protocol.ContextToken.Payload.user)] = userId,
                [nameof(Protocol.ContextToken.Payload.item)] = documentId,
                ["room"] = "9ca05afb1a49f26fb59642305c481661f8b370bd@muc4.virtual-presence.org",
                [nameof(Protocol.ContextToken.Payload.entropy)] = Tools.RandomString.Get(40),
            });
            contextNode.AsDictionary.Add(nameof(Protocol.ContextToken.payload), payloadNode);
            contextNode.AsDictionary.Add(nameof(Protocol.ContextToken.hash), Common.Protocol.ComputePayloadHash(payloadHashSecret, payloadNode));
            var contextToken = contextNode.ToJson().ToBase64();
            return contextToken;
        }

        private static async Task<string> GenerateDeveloperToken(string developerId, SiloSimulatorClusterClient simulatorClient)
        {
            await simulatorClient.Transaction(developerId, async self => {
                await self.Execute(nameof(Aspects.Developer.GenerateToken), new Dictionary<string, string>());
            });
            var developerToken = (string)await simulatorClient.GetItemReader(developerId).Get(Pid.DeveloperToken);
            return developerToken;
        }

        #endregion
    }
}
