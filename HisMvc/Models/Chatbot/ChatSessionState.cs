namespace HisMvc.Models.Chatbot;

/// <summary>Trạng thái phiên chatbot (luồng đặt lịch / hủy lịch).</summary>
public class ChatSessionState
{
    public string Flow { get; set; } = FlowType.None;
    public string Step { get; set; } = "";
    public BookingDraft Booking { get; set; } = new();
    public CancelDraft Cancel { get; set; } = new();
    public List<SelectOption> DepartmentOptions { get; set; } = [];
    public List<SelectOption> DoctorOptions { get; set; } = [];
    public List<SlotOption> SlotOptions { get; set; } = [];
}

public static class FlowType
{
    public const string None = "none";
    public const string Booking = "booking";
    public const string Cancel = "cancel";
}

public static class BookingStep
{
    public const string Department = "department";
    public const string Doctor = "doctor";
    public const string Date = "date";
    public const string Slot = "slot";
    public const string FullName = "fullname";
    public const string Dob = "dob";
    public const string Gender = "gender";
    public const string Phone = "phone";
    public const string Note = "note";
    public const string Confirm = "confirm";
}

public static class CancelStep
{
    public const string Code = "code";
    public const string Phone = "phone";
    public const string Confirm = "confirm";
}

public class BookingDraft
{
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public DateOnly? Date { get; set; }
    public int? TimeSlotId { get; set; }
    public string? SlotLabel { get; set; }
    public string? FullName { get; set; }
    public DateOnly? Dob { get; set; }
    public Entities.Gender? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Note { get; set; }
}

public class CancelDraft
{
    public string? Code { get; set; }
    public string? Phone { get; set; }
    public int? AppointmentId { get; set; }
}

public class SelectOption
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
}

public class SlotOption
{
    public int TimeSlotId { get; set; }
    public string Label { get; set; } = "";
    public int Available { get; set; }
}
