using System.Collections.Generic;

namespace nine3q.Tools
{
    public static class JsonPathExtensions
    {
        public static string ToJson<T>(this List<T> self, JsonPath.SerializerOptions options = null)
        {
            var node = new JsonPath.Node(JsonPath.Node.Type.List);
            foreach (T elem in self) {
                node.AsList.Add(new JsonPath.Node(JsonPath.Node.Type.String, elem.ToString()));
            }
            return node.ToJson(options);
        }

        public static JsonPath.Node Add(this JsonPath.Node self, JsonPath.Node value)
        {
            self.AsList.Add(value);
            return self;
        }

        public static JsonPath.Node Set(this JsonPath.Node self, string key, JsonPath.Node value)
        {
            self.AsDictionary[key] = value;
            return self;
        }

        public static JsonPath.Node Set(this JsonPath.Node self, string key, string value)
        {
            self.AsDictionary[key] = new JsonPath.Node(JsonPath.Node.Type.String, value);
            return self;
        }

        public static JsonPath.Node Set(this JsonPath.Node self, string key, long value)
        {
            self.AsDictionary[key] = new JsonPath.Node(JsonPath.Node.Type.Int, value);
            return self;
        }

        public static JsonPath.Node Set(this JsonPath.Node self, string key, bool value)
        {
            self.AsDictionary[key] = new JsonPath.Node(JsonPath.Node.Type.Bool, value);
            return self;
        }

        public static string Get(this JsonPath.Node self, string key)
        {
            return Get(self, key, "");
        }

        public static string Get(this JsonPath.Node self, string key, string defaultValue)
        {
            if (self.AsDictionary.ContainsKey(key)) {
                return self.AsDictionary[key].AsString;
            }
            return defaultValue;
        }

        public static JsonPath.Node GetNode(this JsonPath.Node self, string key)
        {
            if (self.AsDictionary.ContainsKey(key)) {
                return self.AsDictionary[key];
            }
            return new JsonPath.Node(JsonPath.Node.Type.Dictionary);
        }

        public static bool ContainsKey(this JsonPath.Node self, string key)
        {
            return self.AsDictionary.ContainsKey(key);
        }

        public static JsonPath.Node Remove(this JsonPath.Node self, string key)
        {
            self.AsDictionary.Remove(key);
            return self;
        }

    }
}
