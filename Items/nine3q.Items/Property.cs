using System;

namespace nine3q.Items
{
    public static class Property
    {
        public enum Type
        {
            Unknown = 0,
            Int,
            String,
            Float,
            Bool,
            Item,
            ItemSet,
        }

        public class Definition
        {
            public Pid Id { get; set; }
            public Type Type { get; set; }
        }

        public static Definition GetDefinition(string name)
        {
            return new Definition();
        }

    }
}