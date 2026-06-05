using HisMvc.Entities;

namespace HisMvc.Services;

/// <summary>
/// Khoảng thời gian theo ngày local, quy đổi UTC để so sánh CheckInAt.
/// </summary>
public static class EncounterDayHelper
{
    public static (DateTime StartUtc, DateTime EndUtc) GetLocalTodayUtcRange()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return GetLocalDateUtcRange(today, today);
    }

    public static (DateTime StartUtc, DateTime EndUtc) GetLocalDateUtcRange(DateOnly from, DateOnly toInclusive)
    {
        var startLocal = from.ToDateTime(TimeOnly.MinValue);
        var endLocal = toInclusive.AddDays(1).ToDateTime(TimeOnly.MinValue);
        return (ToUtc(startLocal), ToUtc(endLocal));
    }

    public static IQueryable<Encounter> WhereCheckInToday(IQueryable<Encounter> query)
    {
        var (startUtc, endUtc) = GetLocalTodayUtcRange();
        return query.Where(e => e.CheckInAt >= startUtc && e.CheckInAt < endUtc);
    }

    private static DateTime ToUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(local, TimeZoneInfo.Local);
}
