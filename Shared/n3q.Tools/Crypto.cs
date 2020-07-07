using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace n3q.Tools
{
    public static class Crypto
    {
        public static string SHA1Hex(string input)
        {
            using var sha = new SHA1Managed();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hashBytes.Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
        }

        public static string SHA256Base64(string input)
        {
            using var sha = new SHA256Managed();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
