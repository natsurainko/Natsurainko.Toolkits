using System;

namespace Natsurainko.Toolkits.Values;

public static class DateTimeExtension
{
    public static long ToTimeStamp(this DateTime dateTime, bool isJsTimeStamp = false)
    {
        DateTime startTime = new DateTime(1970, 1, 1).ToLocalTime();

        if (isJsTimeStamp)
            return (long)(DateTime.Now - startTime).TotalMilliseconds;

        return (long)(DateTime.Now - startTime).TotalSeconds;
    }

    public static DateTime ToDateTime(this long timeStamp, bool isJsTimeStamp = false)
    {
        var dateTime = new DateTime(1970, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc);

        if (isJsTimeStamp)
            return dateTime.AddMilliseconds(timeStamp);

        return dateTime.AddMilliseconds(timeStamp);
    }
}
