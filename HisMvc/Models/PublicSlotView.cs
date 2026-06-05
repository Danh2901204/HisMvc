namespace HisMvc.Models;

/// <summary>Khung giờ hiển thị trên cổng đặt lịch (gồm ca đã qua để làm mờ).</summary>
public class PublicSlotView
{
    public int TimeSlotId { get; set; }
    public string? Code { get; set; }
    public string Start { get; set; } = "";
    public string End { get; set; } = "";
    public bool IsPast { get; set; }
    public bool IsFull { get; set; }
    public bool CanBook { get; set; }
}
