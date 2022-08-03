using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Natsurainko.Toolkits.Network
{
    public static class UrlExtension
    {
        public static string Combine(params string[] paths)
            => string.Join("/", paths);
    }
}
