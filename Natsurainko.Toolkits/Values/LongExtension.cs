namespace Natsurainko.Toolkits.Values
{
    public static class LongExtension
    {
        public static string LengthToMb(this long value) => $"{value / (1024.0 * 1024.0):0.0} Mb";
    }
}
