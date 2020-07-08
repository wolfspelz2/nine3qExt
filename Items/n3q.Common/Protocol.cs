using System.Collections.Generic;

namespace n3q.Common
{
    public static class Protocol
    {
        public class PayloadCommon
        {
            public static string entropy;
            public static string expires;
        }

        public class TokenBase
        {
            public static string api;
            public static string hash;
        }

        public class DeveloperToken : TokenBase
        {
            public class Payload : PayloadCommon
            {
                public static string developer;
            }
            public static Payload payload;
        }

        public class ContextToken : TokenBase
        {
            public class Payload : PayloadCommon
            {
                public static string user;
                public static string item;
            }
            public static Payload payload;
        }

        public static class Rpc
        {
            public class Request
            {
                public static string method;
            }

            public class Response
            {
                public static string status;
                public static string status_ok = "ok";
                public static string status_error = "error";
                public static string result;
                public static string message;
            }

            public class DeveloperRequest : Request
            {
                public static string developer;
            }

            public class ItemRequest : DeveloperRequest
            {
                public static string context;
            }

            public class GetItemPropertiesRequest : ItemRequest
            {
                public static List<string> pids;
            }

            public class ExecuteItemActionRequest : ItemRequest
            {
                public static string action;
                public static Dictionary<string, object> args;
            }

        }

        public static string ComputePayloadHash(string secret, JsonPath.Node payloadNode)
        {
            var normalizedNode = payloadNode.Normalized();
            var payloadJson = normalizedNode.ToJson(false, false);
            var data = secret + payloadJson;
            var hash = Tools.Crypto.SHA256Base64(data);
            return hash;
        }
    }
}
