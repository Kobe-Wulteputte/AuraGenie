namespace AuraGenie.Api.Helper;

public static class UnixTimeHelper
{
    public static double ToUnixTime(this DateTime date)
    {
        return Math.Round(date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000);
    }

    public static DateTime FromUnixTime(double unixTime)
    {
        var time = unixTime / 1000;
        return new DateTime(1970, 1, 1).AddSeconds(time);
    }
}