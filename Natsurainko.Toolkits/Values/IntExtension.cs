using System;

namespace Natsurainko.Toolkits.Values;

public static class IntExtension
{
    public static string FormatUnit(this int value)
    {
        double d = value;
        int i = 0;

        while ((d > 1000) && (i < 2))
        {
            d /= 1000;
            i++;
        }

        var unit = new string[] { "", "K", "M" };
        return string.Format("{0}{1}", Math.Round(d, 2), unit[i]);
    }
}
