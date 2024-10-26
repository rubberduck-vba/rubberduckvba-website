namespace rubberduckvba.com.Server.Services;

public static class DateTimeExtensions
{
    public static readonly string TimestampFormatString = "yyyy-MM-dd hh:mm:ss.fff";

    public static string ToTimestampString(this DateTime value) => value.ToString(TimestampFormatString);
    public static string ToTimestampString(this DateTimeOffset value) => value.ToString(TimestampFormatString);

}
