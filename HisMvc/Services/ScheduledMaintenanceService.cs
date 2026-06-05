namespace HisMvc.Services;

/// <summary>
/// Dọn dẹp dữ liệu quá hạn: lịch hẹn NoShow và lượt khám treo từ ngày trước.
/// </summary>
public static class ScheduledMaintenanceService
{
    public static async Task RunAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var appointmentCancellation = services.GetRequiredService<IAppointmentCancellationService>();
        var encounterCancellation = services.GetRequiredService<IEncounterCancellationService>();

        await appointmentCancellation.MarkOverdueAsNoShowAsync(ct);
        await encounterCancellation.CancelStaleActiveEncountersAsync(ct);
    }
}
