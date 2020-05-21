namespace nine3q.Web
{
    public class PropertyFilter
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
