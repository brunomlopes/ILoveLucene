using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Core.Extensions
{
    public static class Hash
    {
        public static string HashStrings(params string[] strings)
        {
            var sha1 = SHA1.Create();
            sha1.Initialize();

            var buffer = strings
                .SelectMany(Encoding.UTF8.GetBytes)
                .ToArray();

            return
                BitConverter.ToString(sha1.ComputeHash(buffer))
                    .Replace("-", "");
        }
    }
}