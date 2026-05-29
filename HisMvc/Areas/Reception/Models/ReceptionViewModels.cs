using HisMvc.Entities;
using HisMvc.Models;

namespace HisMvc.Areas.Reception.Models;

public class ReceptionDashboardViewModel
{
    public ReceptionKpiViewModel Kpi { get; set; } = new();
    public List<Appointment> UpcomingToday { get; set; } = new();
    public List<DashboardActivity> Activities { get; set; } = new();
}

public class ReceptionKpiViewModel
{
    public int BookedToday { get; set; }
    public int CheckedInToday { get; set; }
    public int CancelledToday { get; set; }
    public int NoShowToday { get; set; }
    public int NewPatients { get; set; }
    public int BookedNext7 { get; set; }
}

public class AppointmentListViewModel
{
    public List<Appointment> Appointments { get; set; } = new();
    public DateOnly SelectedDate { get; set; }
    public string CurrentStatus { get; set; } = "";
    public List<int> CheckedInAppointmentIds { get; set; } = new();
}
