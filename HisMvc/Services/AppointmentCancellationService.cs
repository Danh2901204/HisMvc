using HisMvc.Data;
using HisMvc.Entities;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services;

public record AppointmentCancelResult(bool Success, string Message);

public interface IAppointmentCancellationService
{
    Task<AppointmentCancelResult> CancelByReceptionAsync(int appointmentId, CancellationToken ct = default);
    Task<AppointmentCancelResult> CancelByPatientAsync(string code, string phone, CancellationToken ct = default);
    Task<int> MarkOverdueAsNoShowAsync(CancellationToken ct = default);
}

/// <summary>Hủy lịch hẹn (lễ tân / bệnh nhân) và đánh dấu NoShow tự động.</summary>
public class AppointmentCancellationService : IAppointmentCancellationService
{
    public const int NoShowGraceMinutes = 60;

    private readonly AppDbContext _db;
    private readonly ILogger<AppointmentCancellationService> _logger;

    public AppointmentCancellationService(AppDbContext db, ILogger<AppointmentCancellationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<AppointmentCancelResult> CancelByReceptionAsync(int appointmentId, CancellationToken ct = default)
    {
        var appointment = await _db.Appointments
            .Include(a => a.TimeSlot)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct);

        if (appointment == null)
            return new AppointmentCancelResult(false, "Không tìm thấy lịch hẹn");

        var blockReason = await GetCancelBlockReasonAsync(appointment, ct);
        if (blockReason != null)
            return new AppointmentCancelResult(false, blockReason);

        ApplyCancelled(appointment);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Reception cancelled appointment {Code} (Id={Id})", appointment.Code, appointment.AppointmentId);
        return new AppointmentCancelResult(true, "Đã hủy lịch hẹn thành công");
    }

    public async Task<AppointmentCancelResult> CancelByPatientAsync(string code, string phone, CancellationToken ct = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedPhone = phone.Trim();

        var appointment = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.TimeSlot)
            .FirstOrDefaultAsync(a => a.Code == normalizedCode, ct);

        if (appointment == null)
            return new AppointmentCancelResult(false, "Không tìm thấy lịch hẹn");

        if (appointment.Patient?.Phone != normalizedPhone)
            return new AppointmentCancelResult(false, "Số điện thoại không khớp với lịch hẹn");

        var blockReason = await GetCancelBlockReasonAsync(appointment, ct);
        if (blockReason != null)
            return new AppointmentCancelResult(false, blockReason);

        ApplyCancelled(appointment);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Patient cancelled appointment {Code}", appointment.Code);
        return new AppointmentCancelResult(true, "Đã hủy lịch hẹn thành công");
    }

    public async Task<int> MarkOverdueAsNoShowAsync(CancellationToken ct = default)
    {
        var now = AppointmentSlotRules.GetHospitalNow();
        var today = DateOnly.FromDateTime(now);

        var candidates = await _db.Appointments
            .Include(a => a.TimeSlot)
            .Where(a => a.Status == AppointmentStatus.Booked && a.Date <= today)
            .ToListAsync(ct);

        if (candidates.Count == 0)
            return 0;

        var candidateIds = candidates.Select(a => a.AppointmentId).ToList();
        var checkedInIds = await _db.Encounters
            .Where(e => e.AppointmentId.HasValue && candidateIds.Contains(e.AppointmentId.Value))
            .Select(e => e.AppointmentId!.Value)
            .ToListAsync(ct);
        var checkedInSet = checkedInIds.ToHashSet();

        var marked = 0;
        foreach (var appt in candidates)
        {
            if (checkedInSet.Contains(appt.AppointmentId))
                continue;

            if (appt.TimeSlot == null)
                continue;

            var deadline = appt.Date.ToDateTime(appt.TimeSlot.Start).AddMinutes(NoShowGraceMinutes);
            if (now <= deadline)
                continue;

            appt.Status = AppointmentStatus.NoShow;
            appt.NoShowAt = DateTime.UtcNow;
            marked++;
            _logger.LogInformation("Auto NoShow appointment {Code} (deadline {Deadline:yyyy-MM-dd HH:mm})",
                appt.Code, deadline);
        }

        if (marked > 0)
            await _db.SaveChangesAsync(ct);

        return marked;
    }

    private async Task<string?> GetCancelBlockReasonAsync(Appointment appointment, CancellationToken ct)
    {
        if (appointment.IsCancelled)
            return "Lịch hẹn đã hủy trước đó";

        if (appointment.IsNoShow)
            return "Lịch hẹn đã được đánh dấu không đến khám";

        if (appointment.Status != AppointmentStatus.Booked)
            return "Không thể hủy lịch đã check-in hoặc đã hoàn tất";

        if (await _db.Encounters.AnyAsync(e => e.AppointmentId == appointment.AppointmentId, ct))
            return "Không thể hủy lịch hẹn đã check-in. Vui lòng hủy lượt khám trong hệ thống.";

        return null;
    }

    private static void ApplyCancelled(Appointment appointment)
    {
        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
    }
}
