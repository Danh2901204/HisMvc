using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HisMvc.Areas.Doctor.Models;

public class DoctorDashboardViewModel
{
    public DoctorKpiViewModel Kpi { get; set; } = new();
    public List<Encounter> Queue { get; set; } = new();
    public List<DashboardActivity> Activities { get; set; } = new();
}

public class DoctorKpiViewModel
{
    public int Waiting { get; set; }
    public int InProgress { get; set; }
    public int WaitingResult { get; set; }
    public int DoneToday { get; set; }
    public int MyAdmissions { get; set; }
    public int PendingOrders { get; set; }
}

public class EncounterListViewModel
{
    public List<Encounter> Encounters { get; set; } = new();
    public string CurrentStatus { get; set; } = "";
}

public class ExamineViewModel
{
    public Encounter Encounter { get; set; } = null!;
    public List<Order> Orders { get; set; } = new();
    public Prescription? Prescription { get; set; }
    public List<Allergy> Allergies { get; set; } = new();
    public List<MedicalHistory> MedicalHistories { get; set; } = new();
    public List<Encounter> PreviousEncounters { get; set; } = new();
    public SelectList Services { get; set; } = null!;
    public SelectList Medicines { get; set; } = null!;
}
