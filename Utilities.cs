using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNGB
{
    public static class Utilities
    {
        // Source: https://stackoverflow.com/a/42448755, Tomas Kubes
        public static string ToMD5(this string s)
        {
            using var provider = System.Security.Cryptography.MD5.Create();
            StringBuilder builder = new StringBuilder();

            foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(s)))
                builder.Append(b.ToString("x2").ToUpper());

            return builder.ToString();
        }
    }
}
