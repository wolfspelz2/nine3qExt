namespace n3q.Frontend
{
    public static class PropertyFilter
    {
        public static string Url(string url)
        {
            return url
                .Replace("{item.nine3q}", "https://nine3q.dev.sui.li/images/Items/")
                //.Replace("{item.nine3q}", "http://localhost:5000/images/Items/")

                .Replace("{avatar.zweitgeist}", "http://avatar.zweitgeist.com/")
                ;
        }
    }
}
