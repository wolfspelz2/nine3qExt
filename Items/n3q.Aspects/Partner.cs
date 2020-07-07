using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class PartnerExtensions
    {
        public static Partner AsPartner(this ItemStub self) { return new Partner(self); }
    }

    public class Partner : Aspect
    {
        public Partner(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.PartnerAspect;

        public enum Action { GenerateToken }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(Action.GenerateToken), new ActionDescription() { Handler = async (args) => await GenerateToken() } },
            };
        }

        public const string PayloadHashSecretConfigName = "PayloadHashSecret";
        public const string ItemServiceWebApiUrlConfigName = "ItemServiceWebApiUrl";

        public async Task GenerateToken()
        {
            await AssertAspect(Pid.PartnerAspect);

            var config = await Item(Common.ItemService.SiloConfigItemId);
            var configJson = (string)await config.Get(Pid.DocumentText);
            var configNode = new JsonPath.Node(configJson);

            var payloadHashSecret = configNode.AsDictionary[PayloadHashSecretConfigName].String;
            if (!Has.Value(payloadHashSecret)) { throw new Exception($"No {PayloadHashSecretConfigName}"); }
            var itemServiceWebApiUrl = configNode.AsDictionary[ItemServiceWebApiUrlConfigName].String;
            if (!Has.Value(itemServiceWebApiUrl)) { throw new Exception($"No {ItemServiceWebApiUrlConfigName}"); }

            var tokenNode = new JsonPath.Node(JsonPath.Node.Type.Dictionary);

            tokenNode.AsDictionary.Add("api", itemServiceWebApiUrl);

            var payloadNode = new JsonPath.Node(new Dictionary<string, string> {
                ["partner"] = this.Id,
                ["entropy"] = Tools.RandomString.Get(40)
            });
            tokenNode.AsDictionary.Add("payload", payloadNode);

            var payloadJson = payloadNode.ToJson(bFormatted: false, bWrapped: false);
            var hash = ComputePayloadHash(payloadHashSecret, payloadJson);
            tokenNode.AsDictionary.Add("hash", hash);

            var tokenJson = tokenNode.ToJson(bFormatted: false, bWrapped: false);
            var token = Tools.Base64.Encode(tokenJson);

            await this.Set(Pid.PartnerToken, token);
        }

        public static string ComputePayloadHash(string secret, string payload)
        {
            var data = secret + payload;
            var hash = Tools.Crypto.SHA256Base64(data);
            return hash;
        }
    }
}
