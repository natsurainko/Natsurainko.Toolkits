using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Natsurainko.Toolkits.Text
{
    public static class StringExtension
    {
        public static string Replace(this string raw, Dictionary<string, string> keyValuePairs)
        {
            string cache = raw;

            foreach (var item in keyValuePairs)
                cache = cache.Replace(item.Key, item.Value);

            return cache;
        }

        public static string ToPath(this string raw)
            => raw.Contains(' ') ? $"\"{raw}\"" : raw;
    }
}
