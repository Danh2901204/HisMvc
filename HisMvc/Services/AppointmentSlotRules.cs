namespace HisMvc.Services;

/// <summary>Quy tắc khung giờ khám theo giờ thực (múi giờ VN).</summary>
public static class AppointmentSlotRules
{
    private static readonly TimeZoneInfo? VietnamTimeZone = ResolveVietnamTimeZone();

    public static DateTime GetHospitalNow()
    {
        if (VietnamTimeZone != null)
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);

        return DateTime.Now;
    }

    public static DateOnly GetHospitalToday() => DateOnly.FromDateTime(GetHospitalNow());

    /// <summary>Ca đã qua nếu ngày trong quá khứ, hoặc cùng ngày mà giờ bắt đầu ca &lt;= giờ hiện tại.</summary>
    public static bool IsPast(DateOnly appointmentDate, TimeOnly slotStart, DateTime? referenceNow = null)
    {
        var now = referenceNow ?? GetHospitalNow();
        var today = DateOnly.FromDateTime(now);

        if (appointmentDate < today) return true;
        if (appointmentDate > today) return false;

        return slotStart <= TimeOnly.FromDateTime(now);
    }

    private static TimeZoneInfo? ResolveVietnamTimeZone()
    {
        foreach (var id in new[] { "SE Asia Standard Time", "Asia/Ho_Chi_Minh" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
                // thử id tiếp theo
            }
            catch (InvalidTimeZoneException)
            {
                // thử id tiếp theo
            }
        }

        return null;
    }
}
