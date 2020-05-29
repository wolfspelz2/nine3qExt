namespace n3q.Common
{
    public static class PropertyFilter
    {
        public static string ItemBase = "{item.nine3q}";

        public static string Url(string url)
        {
            return url
                //.Replace(ItemBase, "https://nine3q.dev.sui.li/images/Items/")
                .Replace(ItemBase, "http://localhost:5000/images/Items/")

                .Replace("{avatar.zweitgeist}", "http://avatar.zweitgeist.com/")
                ;
        }
    }
}
