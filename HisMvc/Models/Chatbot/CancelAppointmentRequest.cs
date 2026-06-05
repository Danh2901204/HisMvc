namespace HisMvc.Models.Chatbot;

/// <summary>Yêu cầu hủy lịch hẹn công khai (xác thực bằng mã + SĐT).</summary>
public class CancelAppointmentRequest
{
    public string Code { get; set; } = "";
    public string Phone { get; set; } = "";
}
