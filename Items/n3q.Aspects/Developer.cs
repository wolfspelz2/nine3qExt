﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using n3q.Common;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class DeveloperExtensions
    {
        public static Developer AsDeveloper(this ItemStub self) { return new Developer(self); }
    }

    public class Developer : Aspect
    {
        public Developer(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.DeveloperAspect;

        public override ActionList GetActionList()
        {
            return new ActionList() {
                { nameof(GenerateToken), new ActionDescription() { Handler = async (args) => await GenerateToken() } },
            };
        }

        public const string PayloadHashSecretConfigName = "PayloadHashSecret";
        public const string ItemServiceWebApiUrlConfigName = "ItemServiceWebApiUrl";

        public async Task GenerateToken()
        {
            await AssertAspect(Pid.DeveloperAspect);

            var config = ReadonlyItem(Common.ItemService.WebItConfigItemId);
            var configJson = (string)await config.Get(Pid.DocumentText);
            var configNode = new JsonPath.Node(configJson);

            var payloadHashSecret = configNode.AsDictionary[PayloadHashSecretConfigName].String;
            if (!Has.Value(payloadHashSecret)) { throw new Exception($"No {PayloadHashSecretConfigName}"); }
            var itemServiceWebApiUrl = configNode.AsDictionary[ItemServiceWebApiUrlConfigName].String;
            if (!Has.Value(itemServiceWebApiUrl)) { throw new Exception($"No {ItemServiceWebApiUrlConfigName}"); }

            var tokenNode = new JsonPath.Node(JsonPath.Node.Type.Dictionary);

            tokenNode.AsDictionary.Add(nameof(Protocol.DeveloperToken.api), itemServiceWebApiUrl);

            var payloadNode = new JsonPath.Node(new Dictionary<string, string> {
                [nameof(Protocol.DeveloperToken.Payload.developer)] = this.Id,
                [nameof(Protocol.DeveloperToken.Payload.entropy)] = Tools.RandomString.Get(40)
            });
            tokenNode.AsDictionary.Add(nameof(Protocol.DeveloperToken.payload), payloadNode);

            var payloadJson = payloadNode.ToJson(bFormatted: false, bWrapped: false);
            var hash = Common.Protocol.ComputePayloadHash(payloadHashSecret, payloadJson);
            tokenNode.AsDictionary.Add(nameof(Protocol.DeveloperToken.hash), hash);

            var tokenJson = tokenNode.ToJson(bFormatted: false, bWrapped: false);
            var token = tokenJson.ToBase64();

            await this.Set(Pid.DeveloperToken, token);
        }
    }
}