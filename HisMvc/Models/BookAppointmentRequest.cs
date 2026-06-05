using HisMvc.Entities;

namespace HisMvc.Models;

public class BookAppointmentRequest
{
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateOnly? Dob { get; set; }
    public Gender? Gender { get; set; }
    public int DepartmentId { get; set; }
    public int? DoctorId { get; set; }
    public DateOnly Date { get; set; }
    public int TimeSlotId { get; set; }
    public string? Note { get; set; }
}
