using System.Text.RegularExpressions;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Models.Chatbot;
using HisMvc.Services.Chatbot;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services;

public interface IPublicAppointmentService
{
    Task<IReadOnlyList<SelectOption>> GetBookableDepartmentsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SelectOption>> GetDoctorsAsync(int departmentId, CancellationToken ct = default);
    Task<IReadOnlyList<PublicSlotView>> GetPublicSlotsAsync(DateOnly date, int departmentId, int? doctorId, CancellationToken ct = default);
    Task<PublicBookResult> BookAsync(BookAppointmentRequest request, CancellationToken ct = default);
    Task<PublicCancelResult> CancelAsync(string code, string phone, CancellationToken ct = default);
    Task<Appointment?> FindByCodeAsync(string code, bool tracking, CancellationToken ct = default);
    Task<CancelEligibilityResult> CheckCancelEligibilityAsync(string code, string phone, CancellationToken ct = default);
    Task<bool> IsBookableDepartmentAsync(int departmentId, CancellationToken ct = default);
}

public record PublicBookResult(bool Success, string Message, string? Code, int? AppointmentId = null);

public record PublicCancelResult(bool Success, string Message);

public record CancelEligibilityResult(bool CanCancel, string Message, Appointment? Appointment);

/// <summary>Logic đặt/hủy lịch công khai — dùng chung API portal và chatbot.</summary>
public class PublicAppointmentService : IPublicAppointmentService
{
    public const int MaxCapacity = 10;
    public static readonly DateOnly MinAppointmentDate = new(2026, 1, 1);
    private static readonly Regex AptCodeRegex = new(@"^APT\d{10,20}$", RegexOptions.Compiled);

    private readonly AppDbContext _db;
    private readonly ILogger<PublicAppointmentService> _logger;
    private readonly IAppointmentCancellationService _cancellation;

    public PublicAppointmentService(
        AppDbContext db,
        ILogger<PublicAppointmentService> logger,
        IAppointmentCancellationService cancellation)
    {
        _db = db;
        _logger = logger;
        _cancellation = cancellation;
    }

    public async Task<IReadOnlyList<SelectOption>> GetBookableDepartmentsAsync(CancellationToken ct = default)
    {
        var departments = await DepartmentBookingRules
            .BookableForPublic(_db.Departments)
            .Where(d => d.Code != "DEFAULT")
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

        return departments
            .GroupBy(d => d.Code, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderBy(d => d.DepartmentId).First())
            .OrderBy(d => d.Name)
            .Select(d => new SelectOption { Id = d.DepartmentId, Label = d.Name })
            .ToList();
    }

    public async Task<IReadOnlyList<SelectOption>> GetDoctorsAsync(int departmentId, CancellationToken ct = default) =>
        await _db.Staffs
            .Where(s => s.StaffType == "DOCTOR" && s.IsActive && s.DepartmentId == departmentId)
            .OrderBy(s => s.FullName)
            .Select(s => new SelectOption { Id = s.StaffId, Label = s.FullName })
            .ToListAsync(ct);

    public Task<bool> IsBookableDepartmentAsync(int departmentId, CancellationToken ct = default) =>
        _db.Departments
            .AsNoTracking()
            .AnyAsync(d => d.DepartmentId == departmentId && d.Kind == DepartmentKind.Clinical, ct);

    public async Task<IReadOnlyList<PublicSlotView>> GetPublicSlotsAsync(
        DateOnly date,
        int departmentId,
        int? doctorId,
        CancellationToken ct = default)
    {
        var now = AppointmentSlotRules.GetHospitalNow();
        var timeSlots = await _db.TimeSlots.AsNoTracking().OrderBy(x => x.Start).ToListAsync(ct);
        var bookedCounts = await GetBookedCountsBySlotAsync(date, departmentId, doctorId, ct);

        return timeSlots
            .Select(slot =>
            {
                bookedCounts.TryGetValue(slot.TimeSlotId, out var bookedCount);
                var isFull = bookedCount >= MaxCapacity;
                var isPast = AppointmentSlotRules.IsPast(date, slot.Start, now);
                return new PublicSlotView
                {
                    TimeSlotId = slot.TimeSlotId,
                    Code = slot.Code,
                    Start = slot.Start.ToString("HH:mm"),
                    End = slot.End.ToString("HH:mm"),
                    IsPast = isPast,
                    IsFull = isFull,
                    CanBook = !isPast && !isFull
                };
            })
            .ToList();
    }

    private async Task<Dictionary<int, int>> GetBookedCountsBySlotAsync(
        DateOnly date,
        int departmentId,
        int? doctorId,
        CancellationToken ct)
    {
        var query = _db.Appointments
            .AsNoTracking()
            .Where(x => x.Date == date && x.Status == AppointmentStatus.Booked && x.DepartmentId == departmentId);

        if (doctorId.HasValue)
            query = query.Where(x => x.DoctorId == doctorId);

        return await query
            .GroupBy(x => x.TimeSlotId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
    }

    public async Task<PublicBookResult> BookAsync(BookAppointmentRequest request, CancellationToken ct = default)
    {
        try
        {
            var validation = ValidateBookRequest(request);
            if (validation != null)
                return new PublicBookResult(false, validation, null);

            var department = await _db.Departments.FindAsync([request.DepartmentId], ct);
            if (department == null || !department.AllowsPublicBooking)
                return new PublicBookResult(false, "Khoa/phòng không hỗ trợ đặt lịch khám trực tuyến", null);

            if (!request.DoctorId.HasValue)
                return new PublicBookResult(false, "Vui lòng chọn bác sĩ khám", null);

            var doctor = await _db.Staffs.FirstOrDefaultAsync(s =>
                s.StaffId == request.DoctorId.Value &&
                s.StaffType == "DOCTOR" &&
                s.IsActive &&
                s.DepartmentId == request.DepartmentId, ct);

            if (doctor == null)
                return new PublicBookResult(false, "Bác sĩ không hợp lệ hoặc không thuộc chuyên khoa đã chọn", null);

            var timeSlot = await _db.TimeSlots.FindAsync([request.TimeSlotId], ct);
            if (timeSlot == null)
                return new PublicBookResult(false, "Khung giờ khám không hợp lệ", null);

            if (AppointmentSlotRules.IsPast(request.Date, timeSlot.Start))
                return new PublicBookResult(false, "Khung giờ này đã qua. Vui lòng chọn ca buổi chiều hoặc ngày khác.", null);

            var phone = request.Phone.Trim();
            var patient = await _db.Patients.FirstOrDefaultAsync(x => x.Phone == phone, ct);
            if (patient == null)
            {
                patient = new Patient
                {
                    FullName = request.FullName.Trim(),
                    Phone = phone,
                    Dob = request.Dob,
                    Gender = request.Gender!.Value
                };
                _db.Patients.Add(patient);
            }
            else
            {
                patient.FullName = request.FullName.Trim();
                patient.Dob = request.Dob;
                patient.Gender = request.Gender!.Value;
            }

            await _db.SaveChangesAsync(ct);

            var bookedCount = await _db.Appointments.CountAsync(x =>
                x.Date == request.Date &&
                x.TimeSlotId == request.TimeSlotId &&
                x.DepartmentId == request.DepartmentId &&
                x.DoctorId == request.DoctorId &&
                x.Status == AppointmentStatus.Booked, ct);

            if (bookedCount >= MaxCapacity)
                return new PublicBookResult(false, "Khung giờ đã đầy, vui lòng chọn khung giờ khác", null);

            var code = $"APT{DateTime.Now:yyyyMMddHHmmss}";
            var appointment = new Appointment
            {
                Code = code,
                PatientId = patient.PatientId,
                DepartmentId = request.DepartmentId,
                DoctorId = request.DoctorId,
                Date = request.Date,
                TimeSlotId = request.TimeSlotId,
                Status = AppointmentStatus.Booked,
                Note = request.Note?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync(ct);

            return new PublicBookResult(true, "Đặt lịch thành công", code, appointment.AppointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi đặt lịch công khai");
            return new PublicBookResult(false, "Lỗi máy chủ khi đặt lịch", null);
        }
    }

    public async Task<CancelEligibilityResult> CheckCancelEligibilityAsync(string code, string phone, CancellationToken ct = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedPhone = phone.Trim();

        if (!AptCodeRegex.IsMatch(normalizedCode))
            return new CancelEligibilityResult(false, "Mã lịch hẹn không hợp lệ", null);

        if (!ChatbotTextHelper.IsValidPhone(normalizedPhone))
            return new CancelEligibilityResult(false, "Số điện thoại không hợp lệ", null);

        var appointment = await FindByCodeAsync(normalizedCode, tracking: false, ct);
        if (appointment == null)
            return new CancelEligibilityResult(false, "Không tìm thấy lịch hẹn", null);

        if (appointment.Patient?.Phone != normalizedPhone)
            return new CancelEligibilityResult(false, "Số điện thoại không khớp với lịch hẹn", null);

        if (appointment.Status == AppointmentStatus.Cancelled)
            return new CancelEligibilityResult(false, "Lịch hẹn đã được hủy trước đó", appointment);

        if (appointment.Status == AppointmentStatus.NoShow)
            return new CancelEligibilityResult(false, "Lịch hẹn đã được đánh dấu không đến khám", appointment);

        if (appointment.Status != AppointmentStatus.Booked)
            return new CancelEligibilityResult(false, "Không thể hủy lịch đã check-in hoặc đã hoàn tất", appointment);

        if (await _db.Encounters.AnyAsync(e => e.AppointmentId == appointment.AppointmentId, ct))
            return new CancelEligibilityResult(false, "Lịch đã check-in, vui lòng liên hệ quầy Tiếp đón", appointment);

        return new CancelEligibilityResult(true, "OK", appointment);
    }

    public async Task<PublicCancelResult> CancelAsync(string code, string phone, CancellationToken ct = default)
    {
        try
        {
            var result = await _cancellation.CancelByPatientAsync(code, phone, ct);
            return new PublicCancelResult(result.Success, result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi hủy lịch công khai");
            return new PublicCancelResult(false, "Lỗi máy chủ");
        }
    }

    public Task<Appointment?> FindByCodeAsync(string code, bool tracking, CancellationToken ct = default)
    {
        var query = _db.Appointments
            .Include(x => x.Patient)
            .Include(x => x.Department)
            .Include(x => x.Doctor)
            .Include(x => x.TimeSlot)
            .Where(x => x.Code == code.Trim().ToUpperInvariant());

        if (!tracking)
            query = query.AsNoTracking();

        return query.FirstOrDefaultAsync(ct);
    }

    private static string? ValidateBookRequest(BookAppointmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone))
            return "Vui lòng nhập đầy đủ họ tên và số điện thoại";

        if (request.Dob == null)
            return "Vui lòng nhập ngày tháng năm sinh";

        if (request.Gender is null or Gender.Unknown)
            return "Vui lòng chọn giới tính";

        if (!ChatbotTextHelper.IsValidPhone(request.Phone.Trim()))
            return "Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0";

        if (request.Date < MinAppointmentDate)
            return "Ngày khám phải từ năm 2026 trở đi";

        return null;
    }
}
