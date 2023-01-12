using System;
using System.Collections.Generic;

namespace Natsurainko.Toolkits.Values;

public static class LongExtension
{
    public static string FormatSize(this long value)
    {
        double d = value;
        int i = 0;

        while ((d > 1024) && (i < 5))
        {
            d /= 1024;
            i++;
        }

        var unit = new string[] { "B", "KB", "MB", "GB", "TB" };
        return (string.Format("{0} {1}", Math.Round(d, 2), unit[i]));
    }

    public static IEnumerable<(long, long)> SplitIntoRange(this long value, int rangeCount)
    {
        long a = 0;

        while (value > a)
        {
            long add = value / rangeCount;

            if (a + add > value)
                add = value - a;

            yield return (a, a + add);
            a += add;
        }
    }
}
