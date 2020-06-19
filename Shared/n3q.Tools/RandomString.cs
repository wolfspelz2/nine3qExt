using System;

namespace n3q.Tools
{
    public class RandomString
    {
        static readonly Random _rnd = new Random();
        const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

        public static string Get(int nLen)
        {
            var aChars = new char[nLen];

            for (int i = 0; i < nLen; i++) {
                aChars[i] = Chars[_rnd.Next(Chars.Length)];
            }

            return new string(aChars);
        }
    }

    public class RandomInt
    {
        static readonly Random _rnd = new Random();

        public static int Get(int min, int max)
        {
            return _rnd.Next(min, max);
        }
    }
}
