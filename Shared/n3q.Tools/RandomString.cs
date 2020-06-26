using System;

namespace n3q.Tools
{
    public static class RandomCommon
    {
        public static readonly Random Rnd = new Random();
    }
    
    public static class RandomString
    {
        const string AlphanumChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
        const string AlphanumLowercaseChars = "abcdefghijklmnopqrstuvwxyz1234567890";

        public static string Get(int nLen)
        {
            var aChars = new char[nLen];
            var src = AlphanumChars;

            for (int i = 0; i < nLen; i++) {
                aChars[i] = src[RandomCommon.Rnd.Next(src.Length)];
            }

            return new string(aChars);
        }

        public static string GetAlphanumLowercase(int nLen)
        {
            var aChars = new char[nLen];
            var src = AlphanumLowercaseChars;

            for (int i = 0; i < nLen; i++) {
                aChars[i] = src[RandomCommon.Rnd.Next(src.Length)];
            }

            return new string(aChars);
        }
    }

    public static class RandomInt
    {
        public static int Get(int min, int max)
        {
            return RandomCommon.Rnd.Next(min, max);
        }
    }
}
