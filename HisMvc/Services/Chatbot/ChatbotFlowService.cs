using System.Text.RegularExpressions;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Models.Chatbot;
using HisMvc.Services;
using Microsoft.Extensions.Caching.Memory;

namespace HisMvc.Services.Chatbot;

public class ChatbotFlowService : IChatbotFlowService
{
    private readonly IPublicAppointmentService _appointments;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan FlowTtl = TimeSpan.FromHours(2);

    public ChatbotFlowService(IPublicAppointmentService appointments, IMemoryCache cache)
    {
        _appointments = appointments;
        _cache = cache;
    }

    public bool IsExitCommand(string message) =>
        ChatbotTextHelper.Matches(message, "thoat", "huy bo", "exit", "reset", "bat dau lai", "dung lai");

    public bool IsBookingIntent(string message) =>
        ChatbotTextHelper.Matches(message, "dat lich", "dang ky", "hen kham", "book", "dat lich kham", "dang ky kham");

    public bool IsCancelIntent(string message) =>
        ChatbotTextHelper.Matches(message, "huy lich", "huy hen", "huy cuoc hen", "cancel");

    public bool IsSlotsIntent(string message) =>
        ChatbotTextHelper.Matches(message, "khung gio", "gio trong", "con lich", "lich trong", "slot", "gio kham");

    public void ClearFlow(string sessionId) => _cache.Remove(FlowKey(sessionId));

    public async Task<ChatResponse?> TryContinueFlowAsync(string sessionId, string message, CancellationToken ct)
    {
        var state = GetState(sessionId);
        if (state.Flow == FlowType.None) return null;

        return state.Flow switch
        {
            FlowType.Booking => await ContinueBookingAsync(sessionId, state, message, ct),
            FlowType.Cancel => await ContinueCancelAsync(sessionId, state, message, ct),
            _ => null
        };
    }

    public async Task<ChatResponse> StartBookingFlowAsync(string sessionId, CancellationToken ct)
    {
        var depts = (await _appointments.GetBookableDepartmentsAsync(ct)).ToList();
        if (depts.Count == 0)
        {
            ClearFlow(sessionId);
            return Reply("Hiện chưa có chuyên khoa nào hỗ trợ đặt lịch online. Vui lòng gọi **1900 1009**.");
        }

        var state = NewState(FlowType.Booking, BookingStep.Department);
        state.DepartmentOptions = depts;
        SaveState(sessionId, state);

        return Reply(
            $"📅 **Đặt lịch khám qua chat** — Bước 1/9\n\nChọn **chuyên khoa** (gõ số thứ tự hoặc tên khoa):\n\n{FormatNumberedList(depts.Select(d => d.Label))}\n\n_Gõ **thoát** để hủy luồng._",
            ChipLabels(depts, 4).Append("Thoát luồng"));
    }

    public Task<ChatResponse> StartCancelFlowAsync(string sessionId, string? code, CancellationToken ct)
    {
        var state = NewState(FlowType.Cancel, CancelStep.Code);

        if (!string.IsNullOrWhiteSpace(code))
        {
            state.Cancel.Code = code.Trim().ToUpperInvariant();
            state.Step = CancelStep.Phone;
            SaveState(sessionId, state);
            return Task.FromResult(Reply(
                $"🔐 **Hủy lịch hẹn** — Bước 2/3\n\nMã: **{state.Cancel.Code}**\n\nVui lòng nhập **số điện thoại** đã đăng ký (10 số, bắt đầu bằng 0).",
                ["Thoát luồng"]));
        }

        SaveState(sessionId, state);
        return Task.FromResult(Reply(
            "🔐 **Hủy lịch hẹn** — Bước 1/3\n\nVui lòng nhập **mã đặt lịch APT** (ví dụ: APT20260527143022).",
            ["Thoát luồng"]));
    }

    public async Task<ChatResponse> QueryAvailableSlotsAsync(string message, CancellationToken ct)
    {
        var depts = (await _appointments.GetBookableDepartmentsAsync(ct)).ToList();
        if (depts.Count == 0)
            return Reply("Chưa có dữ liệu chuyên khoa. Gọi hotline **1900 1009**.");

        var dept = PickOption(depts, message);
        if (dept == null)
        {
            return Reply(
                $"🕐 **Khung giờ khám** — Vui lòng cho biết **chuyên khoa** (số hoặc tên):\n\n{FormatNumberedList(depts.Select(d => d.Label))}",
                ChipLabels(depts, 4));
        }

        var date = ChatbotTextHelper.ParseDate(message) ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        if (date < PublicAppointmentService.MinAppointmentDate)
            return Reply("Ngày khám phải từ **01/01/2026** trở đi. Vui lòng nhập lại ngày (vd: 15/06/2026).");

        var slots = (await _appointments.GetPublicSlotsAsync(date, dept.Id, null, ct)).BookableOnly();
        if (slots.Count == 0)
        {
            return Reply(
                $"Không còn khung giờ khám ngày **{date:dd/MM/yyyy}** tại **{dept.Label}**.\n\nThử ngày khác hoặc gọi **1900 1009**.",
                ["Đặt lịch khám", "Tra cứu APT"]);
        }

        var slotLines = string.Join("\n", slots.Take(12).Select((s, i) => $"{i + 1}. **{s.Start} - {s.End}**"));
        var overflow = slots.Count > 12 ? $"\n\n_... và {slots.Count - 12} khung giờ khác._" : "";

        return Reply(
            $"🕐 **Khung giờ khám**\n\n**Khoa:** {dept.Label}\n**Ngày:** {date:dd/MM/yyyy}\n\n{slotLines}{overflow}\n\nNhắn **\"đặt lịch\"** để đặt ngay trong chat.",
            ["Đặt lịch khám", "Tra cứu APT", "Hủy lịch"]);
    }

    private async Task<ChatResponse> ContinueBookingAsync(string sessionId, ChatSessionState state, string message, CancellationToken ct)
    {
        var draft = state.Booking;

        switch (state.Step)
        {
            case BookingStep.Department:
                return await PickDepartmentAsync(sessionId, state, message, ct);

            case BookingStep.Doctor:
                return await PickDoctorAsync(sessionId, state, message, ct);

            case BookingStep.Date:
                return await PickDateAsync(sessionId, state, message, ct);

            case BookingStep.Slot:
                return PickSlotStep(sessionId, state, message);

            case BookingStep.FullName:
                return PickFullName(sessionId, state, message);

            case BookingStep.Dob:
                return PickDob(sessionId, state, message);

            case BookingStep.Gender:
                return PickGender(sessionId, state, message);

            case BookingStep.Phone:
                return PickPhone(sessionId, state, message);

            case BookingStep.Note:
                Advance(state, BookingStep.Confirm);
                if (!ChatbotTextHelper.Matches(message, "khong", "bo qua", "skip", "none"))
                    draft.Note = message.Trim();
                SaveState(sessionId, state);
                return Reply(
                    $"{BuildBookingSummary(draft)}\n\nGõ **xác nhận** để hoàn tất hoặc **thoát** để hủy.",
                    ["Xác nhận", "Thoát luồng"]);

            case BookingStep.Confirm:
                if (!ChatbotTextHelper.IsConfirm(message))
                    return Reply("Gõ **xác nhận** để đặt lịch hoặc **thoát** để hủy.", ["Xác nhận", "Thoát luồng"]);

                var result = await _appointments.BookAsync(new BookAppointmentRequest
                {
                    FullName = draft.FullName!,
                    Phone = draft.Phone!,
                    Dob = draft.Dob,
                    Gender = draft.Gender,
                    DepartmentId = draft.DepartmentId!.Value,
                    DoctorId = draft.DoctorId,
                    Date = draft.Date!.Value,
                    TimeSlotId = draft.TimeSlotId!.Value,
                    Note = draft.Note
                }, ct);

                ClearFlow(sessionId);
                if (!result.Success)
                    return Reply(result.Message);

                return Reply(
                    $"""
                    ✅ **Đặt lịch thành công!**

                    **Mã:** {result.Code}
                    **Bệnh nhân:** {draft.FullName}
                    **Khoa:** {draft.DepartmentName}
                    **Bác sĩ:** {draft.DoctorName}
                    **Ngày:** {draft.Date:dd/MM/yyyy} — **{draft.SlotLabel}**

                    Lưu mã **{result.Code}** để tra cứu hoặc hủy lịch sau này.
                    """,
                    ["Tra cứu APT", "Hủy lịch", "Đặt lịch khám"],
                    $"lookup:{result.Code}");
        }

        return Reply("Luồng đặt lịch gặp lỗi. Gõ **đặt lịch** để thử lại.");
    }

    private async Task<ChatResponse> PickDepartmentAsync(string sessionId, ChatSessionState state, string message, CancellationToken ct)
    {
        var picked = PickOption(state.DepartmentOptions, message);
        if (picked == null)
        {
            return Reply(
                "Không nhận diện được chuyên khoa. Gõ **số thứ tự** hoặc **tên khoa**.",
                ChipLabels(state.DepartmentOptions, 4));
        }

        var doctors = (await _appointments.GetDoctorsAsync(picked.Id, ct)).ToList();
        if (doctors.Count == 0)
        {
            ClearFlow(sessionId);
            return Reply($"Khoa **{picked.Label}** chưa có bác sĩ khám. Chọn khoa khác hoặc gọi **1900 1009**.");
        }

        state.Booking.DepartmentId = picked.Id;
        state.Booking.DepartmentName = picked.Label;
        state.DoctorOptions = doctors;
        Advance(state, BookingStep.Doctor);
        SaveState(sessionId, state);

        return Reply(
            $"📅 Bước 2/9 — Chọn **bác sĩ** tại **{picked.Label}**:\n\n{FormatNumberedList(doctors.Select(d => d.Label))}",
            ChipLabels(doctors, 3));
    }

    private async Task<ChatResponse> PickDoctorAsync(string sessionId, ChatSessionState state, string message, CancellationToken ct)
    {
        var picked = PickOption(state.DoctorOptions, message);
        if (picked == null)
        {
            return Reply(
                "Vui lòng chọn bác sĩ bằng **số** hoặc **tên**.",
                ChipLabels(state.DoctorOptions, 3));
        }

        state.Booking.DoctorId = picked.Id;
        state.Booking.DoctorName = picked.Label;
        Advance(state, BookingStep.Date);
        SaveState(sessionId, state);

        return Reply(
            $"📅 Bước 3/9 — Chọn **ngày khám** (vd: 15/06/2026, ngày mai, hôm nay).\n\n**Bác sĩ:** {picked.Label}",
            ["Ngày mai", "Hôm nay"]);
    }

    private async Task<ChatResponse> PickDateAsync(string sessionId, ChatSessionState state, string message, CancellationToken ct)
    {
        var date = ChatbotTextHelper.ParseDate(message);
        if (date == null || date < PublicAppointmentService.MinAppointmentDate)
            return Reply("Ngày không hợp lệ. Nhập **dd/MM/yyyy** (từ 01/01/2026).", ["Ngày mai"]);

        var draft = state.Booking;
        var bookable = (await _appointments.GetPublicSlotsAsync(date.Value, draft.DepartmentId!.Value, draft.DoctorId, ct))
            .BookableOnly();
        if (bookable.Count == 0)
            return Reply($"Không còn khung giờ ngày **{date:dd/MM/yyyy}**. Chọn **ngày khác**.", ["Ngày mai"]);

        draft.Date = date;
        state.SlotOptions = bookable.ToSlotOptions();
        Advance(state, BookingStep.Slot);
        SaveState(sessionId, state);

        return Reply(
            $"📅 Bước 4/9 — Chọn **giờ khám** ngày **{date:dd/MM/yyyy}**:\n\n{FormatNumberedList(state.SlotOptions.Select(s => s.Label))}",
            ChipLabels(state.SlotOptions.Select(s => s.Label), 4));
    }

    private ChatResponse PickSlotStep(string sessionId, ChatSessionState state, string message)
    {
        var picked = PickSlot(state.SlotOptions, message);
        if (picked == null)
        {
            return Reply(
                "Chọn khung giờ bằng **số** hoặc **giờ** (vd: 08:00).",
                ChipLabels(state.SlotOptions.Select(s => s.Label), 4));
        }

        state.Booking.TimeSlotId = picked.TimeSlotId;
        state.Booking.SlotLabel = picked.Label;
        Advance(state, BookingStep.FullName);
        SaveState(sessionId, state);
        return Reply("📅 Bước 5/9 — Nhập **họ và tên** bệnh nhân (in hoa có dấu).");
    }

    private ChatResponse PickFullName(string sessionId, ChatSessionState state, string message)
    {
        var name = message.Trim();
        if (name.Length < 2)
            return Reply("Họ tên quá ngắn. Vui lòng nhập **họ tên đầy đủ**.");

        state.Booking.FullName = name;
        Advance(state, BookingStep.Dob);
        SaveState(sessionId, state);
        return Reply("📅 Bước 6/9 — Nhập **ngày sinh** (dd/MM/yyyy).");
    }

    private ChatResponse PickDob(string sessionId, ChatSessionState state, string message)
    {
        var dob = ChatbotTextHelper.ParseDate(message);
        if (dob == null || dob > DateOnly.FromDateTime(DateTime.Today))
            return Reply("Ngày sinh không hợp lệ. Nhập **dd/MM/yyyy**.");

        state.Booking.Dob = dob;
        Advance(state, BookingStep.Gender);
        SaveState(sessionId, state);
        return Reply("📅 Bước 7/9 — Chọn **giới tính**: **Nam** hoặc **Nữ** (gõ 1/2).", ["Nam", "Nữ"]);
    }

    private ChatResponse PickGender(string sessionId, ChatSessionState state, string message)
    {
        var gender = ChatbotTextHelper.ParseGender(message);
        if (gender is null or Gender.Unknown)
            return Reply("Chọn **Nam** hoặc **Nữ**.", ["Nam", "Nữ"]);

        state.Booking.Gender = gender;
        Advance(state, BookingStep.Phone);
        SaveState(sessionId, state);
        return Reply("📅 Bước 8/9 — Nhập **số điện thoại** (10 số, bắt đầu bằng 0).");
    }

    private ChatResponse PickPhone(string sessionId, ChatSessionState state, string message)
    {
        var phone = ChatbotTextHelper.NormalizePhone(message);
        if (!ChatbotTextHelper.IsValidPhone(phone))
            return Reply("SĐT không hợp lệ. Nhập **10 chữ số** bắt đầu bằng **0**.");

        state.Booking.Phone = phone;
        Advance(state, BookingStep.Note);
        SaveState(sessionId, state);
        return Reply("📅 Bước 9/9 — **Ghi chú** (triệu chứng, yêu cầu...) hoặc gõ **không** để bỏ qua.", ["Không"]);
    }

    private async Task<ChatResponse> ContinueCancelAsync(string sessionId, ChatSessionState state, string message, CancellationToken ct)
    {
        var draft = state.Cancel;

        switch (state.Step)
        {
            case CancelStep.Code:
            {
                var code = ChatbotTextHelper.ExtractAppointmentCode(message);
                if (code == null)
                    return Reply("Mã không hợp lệ. Nhập mã **APT** (vd: APT20260527143022).", ["Thoát luồng"]);

                draft.Code = code;
                Advance(state, CancelStep.Phone);
                SaveState(sessionId, state);
                return Reply($"Mã **{code}** — Nhập **số điện thoại** đã đăng ký.");
            }

            case CancelStep.Phone:
            {
                var phone = ChatbotTextHelper.NormalizePhone(message);
                if (!ChatbotTextHelper.IsValidPhone(phone))
                    return Reply("SĐT không hợp lệ. Nhập **10 chữ số** bắt đầu bằng **0**.");

                var eligibility = await _appointments.CheckCancelEligibilityAsync(draft.Code!, phone, ct);
                if (!eligibility.CanCancel)
                {
                    ClearFlow(sessionId);
                    return Reply(ToUserCancelMessage(eligibility.Message, draft.Code));
                }

                draft.Phone = phone;
                draft.AppointmentId = eligibility.Appointment!.AppointmentId;
                Advance(state, CancelStep.Confirm);
                SaveState(sessionId, state);

                var appt = eligibility.Appointment;
                var time = appt.TimeSlot != null ? $"{appt.TimeSlot.Start:HH:mm}" : "—";
                return Reply(
                    $"⚠️ **Xác nhận hủy lịch**\n\n**Mã:** {appt.Code}\n**BN:** {appt.Patient?.FullName}\n**Khoa:** {appt.Department?.Name}\n**Ngày:** {appt.Date:dd/MM/yyyy} **{time}**\n\nGõ **xác nhận** để hủy.",
                    ["Xác nhận", "Thoát luồng"]);
            }

            case CancelStep.Confirm:
            {
                if (!ChatbotTextHelper.IsConfirm(message))
                    return Reply("Gõ **xác nhận** để hủy lịch hoặc **thoát** để giữ lịch.", ["Xác nhận", "Thoát luồng"]);

                var result = await _appointments.CancelAsync(draft.Code!, draft.Phone!, ct);
                ClearFlow(sessionId);

                return result.Success
                    ? Reply(
                        $"✅ Đã **hủy lịch** **{draft.Code}** thành công.\n\nBạn có thể **đặt lịch mới** bất cứ lúc nào.",
                        ["Đặt lịch khám", "Tra cứu APT"])
                    : Reply(result.Message);
            }
        }

        return Reply("Luồng hủy lịch gặp lỗi. Gõ **hủy lịch** để thử lại.");
    }

    private static string ToUserCancelMessage(string message, string? code) => message switch
    {
        "Không tìm thấy lịch hẹn" => $"❌ Không tìm thấy lịch **{code}**.",
        "Số điện thoại không khớp với lịch hẹn" => "❌ Số điện thoại **không khớp** với lịch hẹn. Vui lòng kiểm tra lại hoặc gọi **1900 1009**.",
        "Lịch hẹn đã được hủy trước đó" => $"Lịch **{code}** đã được hủy trước đó.",
        "Không thể hủy lịch đã check-in hoặc đã hoàn tất" => "Không thể hủy lịch đã check-in hoặc đã hoàn tất. Gọi **1900 1009**.",
        "Lịch đã check-in, vui lòng liên hệ quầy Tiếp đón" => "Lịch đã check-in, không thể hủy online. Liên hệ quầy Tiếp đón.",
        _ => message
    };

    private static string BuildBookingSummary(BookingDraft draft) =>
        $"""
        📋 **Xác nhận thông tin đặt lịch**

        **Khoa:** {draft.DepartmentName}
        **Bác sĩ:** {draft.DoctorName}
        **Ngày:** {draft.Date:dd/MM/yyyy} — **{draft.SlotLabel}**
        **Họ tên:** {draft.FullName}
        **Ngày sinh:** {draft.Dob:dd/MM/yyyy}
        **Giới tính:** {(draft.Gender == Gender.Male ? "Nam" : "Nữ")}
        **SĐT:** {draft.Phone}
        **Ghi chú:** {(string.IsNullOrWhiteSpace(draft.Note) ? "—" : draft.Note)}
        """;

    private static SelectOption? PickOption(IReadOnlyList<SelectOption> options, string message)
    {
        if (options.Count == 0) return null;

        var index = ChatbotTextHelper.ParseChoiceIndex(message);
        if (index.HasValue && index.Value >= 1 && index.Value <= options.Count)
            return options[index.Value - 1];

        var normalized = ChatbotTextHelper.Normalize(message);
        return options.FirstOrDefault(o => ChatbotTextHelper.Normalize(o.Label).Contains(normalized, StringComparison.Ordinal))
               ?? options.FirstOrDefault(o => normalized.Contains(ChatbotTextHelper.Normalize(o.Label), StringComparison.Ordinal));
    }

    private static SlotOption? PickSlot(IReadOnlyList<SlotOption> options, string message)
    {
        var index = ChatbotTextHelper.ParseChoiceIndex(message);
        if (index.HasValue && index.Value >= 1 && index.Value <= options.Count)
            return options[index.Value - 1];

        var timeMatch = Regex.Match(message, @"(\d{1,2})[:\.]?(\d{2})?");
        if (!timeMatch.Success) return null;

        var hour = timeMatch.Groups[1].Value.PadLeft(2, '0');
        var minute = timeMatch.Groups[2].Success ? timeMatch.Groups[2].Value.PadLeft(2, '0') : "00";
        var needle = $"{hour}:{minute}";
        return options.FirstOrDefault(o => o.Label.StartsWith(needle, StringComparison.Ordinal));
    }

    private static IEnumerable<string> ChipLabels(IEnumerable<SelectOption> options, int take) =>
        options.Take(take).Select(o => o.Label);

    private static IEnumerable<string> ChipLabels(IEnumerable<string> labels, int take) =>
        labels.Take(take);

    private static string FormatNumberedList(IEnumerable<string> items) =>
        string.Join("\n", items.Select((label, i) => $"{i + 1}. {label}"));

    private static void Advance(ChatSessionState state, string step) => state.Step = step;

    private ChatSessionState GetState(string sessionId) =>
        _cache.TryGetValue(FlowKey(sessionId), out ChatSessionState? state) && state != null
            ? state
            : new ChatSessionState();

    private void SaveState(string sessionId, ChatSessionState state) =>
        _cache.Set(FlowKey(sessionId), state, FlowTtl);

    private static ChatSessionState NewState(string flow, string step) =>
        new() { Flow = flow, Step = step, Booking = new BookingDraft(), Cancel = new CancelDraft() };

    private static string FlowKey(string sessionId) => $"chatbot:flow:{sessionId}";

    private static ChatResponse Reply(string reply, IEnumerable<string>? suggestions = null, string? action = null) =>
        ChatbotResponseBuilder.Create(reply, suggestions?.ToList(), action);
}
