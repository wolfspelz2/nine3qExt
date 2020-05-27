using System;

namespace n3q.Tools
{
    public class RandomString
    {
        private static readonly Random Rng = new Random();
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

        public static string Get(int nLen)
        {
            var aChars = new char[nLen];

            for (int i = 0; i < nLen; i++) {
                aChars[i] = Chars[Rng.Next(Chars.Length)];
            }

            return new string(aChars);
        }
    }
}
