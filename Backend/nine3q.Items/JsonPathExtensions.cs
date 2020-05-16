using System.Collections.Generic;

namespace nine3q.Lib
{
    public static class JsonPathExtensions
    {
        public static string ToJson<T>(this List<T> self, JsonPath.Serializer.Options options = null)
        {
            var node = new JsonPath.Node(JsonPath.Node.Type.List);
            foreach (T elem in self) {
                node.AsList.Add(new JsonPath.Node(JsonPath.Node.Type.String, elem.ToString()));
            }
            return node.ToJson(options);
        }

        public static JsonPath.Node Add(this JsonPath.Node _this, JsonPath.Node value)
        {
            _this.AsList.Add(value);
            return _this;
        }

        public static JsonPath.Node Set(this JsonPath.Node _this, string key, JsonPath.Node value)
        {
            _this.AsDictionary[key] = value;
            return _this;
        }

        public static JsonPath.Node Set(this JsonPath.Node _this, string key, string value)
        {
            _this.AsDictionary[key] = new JsonPath.Node(JsonPath.Node.Type.String, value);
            return _this;
        }

        public static JsonPath.Node Set(this JsonPath.Node _this, string key, long value)
        {
            _this.AsDictionary[key] = new JsonPath.Node(JsonPath.Node.Type.Int, value);
            return _this;
        }

        public static JsonPath.Node Set(this JsonPath.Node _this, string key, bool value)
        {
            _this.AsDictionary[key] = new JsonPath.Node(JsonPath.Node.Type.Bool, value);
            return _this;
        }

        public static string Get(this JsonPath.Node _this, string key)
        {
            return Get(_this, key, "");
        }

        public static string Get(this JsonPath.Node _this, string key, string defaultValue)
        {
            if (_this.AsDictionary.ContainsKey(key)) {
                return _this.AsDictionary[key].AsString;
            }
            return defaultValue;
        }

        public static JsonPath.Node GetNode(this JsonPath.Node _this, string key)
        {
            if (_this.AsDictionary.ContainsKey(key)) {
                return _this.AsDictionary[key];
            }
            return new JsonPath.Node(JsonPath.Node.Type.Dictionary);
        }

        public static bool ContainsKey(this JsonPath.Node _this, string key)
        {
            return _this.AsDictionary.ContainsKey(key);
        }

        public static JsonPath.Node Remove(this JsonPath.Node _this, string key)
        {
            _this.AsDictionary.Remove(key);
            return _this;
        }

    }
}
