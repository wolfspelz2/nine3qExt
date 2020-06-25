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
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
        }
    }
}
