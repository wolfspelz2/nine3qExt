using System;
using System.Text;

namespace n3q.Tools
{
    public static class Base64
    {
        public static string Encode(string s)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }

        public static string Decode(string s)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }
    }
}
