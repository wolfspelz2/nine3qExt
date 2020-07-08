using System.Collections.Generic;

namespace n3q.Common
{
    public static class Protocol
    {
        public static class Rpc
        {
            public class Request
            {
                public static string method;
            }

            public static class Response
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

            public static class DeveloperToken
            {
                public static string api;
                public class Payload
                {
                    public static string developer;
                    public static string entropy;
                    public static string expires;
                }          
                public static Payload payload;
                public static string hash;
            }

            public static class ContextToken
            {
                public static string api;
                public class Payload
                {
                    public static string user;
                    public static string item;
                    public static string entropy;
                    public static string expires;
                }
                public static Payload payload;
                public static string hash;
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
    }
}
