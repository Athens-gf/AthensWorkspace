namespace Utility;

public static class DateTimeUtil
{
    /// <summary> JSTのタイムゾーン情報を取得 </summary>
    public static readonly TimeZoneInfo TzJst = GetTimeZone("Asia/Tokyo", "Tokyo Standard Time");

    public static TimeZoneInfo GetTimeZone(params string[] names)
    {
        foreach (var name in names)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(name);
            }
            catch (TimeZoneNotFoundException)
            {
            }
        }

        throw new TimeZoneNotFoundException();
    }

    public static DateTime ToJst(this DateTime dateTime) => TimeZoneInfo.ConvertTime(dateTime, TzJst);

    public static DateTime NowJst => DateTime.Now.ToJst();
    public static DateTime TodayJst => DateTime.Today.ToJst();
}