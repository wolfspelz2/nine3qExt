using System;
using n3q.Common;
using n3q.Tools;

namespace n3q.WebIt
{
    public class ContextToken
    {
        public string UserId { get; private set; }
        public string ItemId { get; private set; }
        public DateTime Expires { get; private set; }

        public static ContextToken FromBase64TokenAndValiate(string payloadHashSecret, string tokenBase64Encoded)
        {
            var ct = new ContextToken();

            if (!Has.Value(tokenBase64Encoded)) { throw new Exception("No context token"); }

            var tokenString = tokenBase64Encoded.FromBase64();
            var tokenNode = new JsonPath.Node(tokenString);

            var hash = tokenNode[nameof(Protocol.ContextToken.hash)].AsString;
            var payloadNode = tokenNode[nameof(Protocol.ContextToken.payload)];
            if (!Has.Value(payloadNode.AsString)) { throw new Exception("No payload"); }
            var computedHash = Common.Protocol.ComputePayloadHash(payloadHashSecret, payloadNode);
            if (hash != computedHash) { throw new Exception("Hash mismatch"); }

            ct.UserId = payloadNode[nameof(Protocol.ContextToken.Payload.user)];
            ct.ItemId = payloadNode[nameof(Protocol.ContextToken.Payload.item)];
            ct.Expires = payloadNode[nameof(Protocol.ContextToken.Payload.expires)];

            if (!Has.Value(ct.UserId)) { throw new Exception("No user in context"); }
            if (!Has.Value(ct.ItemId)) { throw new Exception("No item in context"); }

            return ct;
        }
    }
}