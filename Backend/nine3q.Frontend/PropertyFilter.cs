namespace nine3q.Frontend
{
    public static class PropertyFilter
    {
        public static string Url(string url)
        {
            return url
                .Replace("{item.nine3q}", "http://localhost:5000/images/Items/")
                .Replace("{avatar.zweitgeist}", "http://avatar.zweitgeist.com/")
                ;
        }
    }
}
