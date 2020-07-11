﻿using System.Collections.Generic;

namespace n3q.WebEx
{
    public class WebExConfigDefinition : n3q.Common.ConfigBag
    {
        public string XmppServiceUrl = "wss://xmpp-user-host.example.com/xmpp-websocket";
        public string XmppDomain = "xmpp-user-host.example.com";
        public string XmppUserPasswordSHA1Secret = "3b6f88f2bed0f392";

        public const string DefaultExtensionWebBaseUrl = "https://ex-web.example.com/";
        public const string DefaultItemWebBaseUrl = "https://it-web.example.com/";
        public string ExtensionWebBaseUrl = DefaultExtensionWebBaseUrl;
        public string ItemWebBaseUrl = DefaultItemWebBaseUrl;
        public string IdentificatorUrlTemplate = DefaultExtensionWebBaseUrl + "Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}";
        public string AnimationsProxyUrlTemplate = DefaultExtensionWebBaseUrl + "Avatar/InlineData?url={url}";
        public string ImageProxyUrlTemplate = DefaultExtensionWebBaseUrl + "Avatar/HttpBridge?url={url}";
        public string ItemConfigUrlTemplate = DefaultItemWebBaseUrl + "Config?id={id}";

        public string AnimationsUrlTemplate = "https://avatar.zweitgeist.com/gif/{id}/config.xml";
        public List<string> AvatarProxyPreloadSequenceNames = new List<string> { "idle", "moveright", "moveleft" };
        public long MemoryCacheSizeBytes = 200 * 1024 * 1024;
        public bool UpgradeAvatarXmlUrlToHttps = true;
        public bool UpgradeAvatarImageUrlToHttps = true;
    }
}