namespace Lagrange.Milky.Extension;

public static class DateTimeExtension
{
    public static long ToUnixTimeSeconds(this DateTime time) => new DateTimeOffset(time, TimeSpan.Zero).ToUnixTimeSeconds();
}