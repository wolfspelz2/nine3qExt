using System;

namespace n3q.Items
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

        public enum Access
        {
            Unknown = 0,
            Internal, // Internal sees all
            Owner, // Owner sees all props except the internal props
            Public, // Everyone else sees only the public props
        }

        public class Definition
        {
            public Pid Id { get; set; }
            public Type Type { get; set; }
            public Access Access { get; set; }
        }

        public static Definition GetDefinition(string name)
        {
            return new Definition();
        }

    }
}