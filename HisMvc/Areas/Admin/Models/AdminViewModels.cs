using HisMvc.Entities;
using HisMvc.Models;

namespace HisMvc.Areas.Admin.Models;

public class AdminDashboardViewModel
{
    public AdminKpiViewModel Kpi { get; set; } = new();
    public List<DashboardActivity> Activities { get; set; } = new();
}

public class AdminKpiViewModel
{
    public int TotalDepartments { get; set; }
    public int TotalStaff { get; set; }
    public int TotalServices { get; set; }
    public int TotalPatients { get; set; }
    public int TotalAppointmentsToday { get; set; }
    public int TotalEncountersToday { get; set; }
    public int ActiveAdmissions { get; set; }
    public int UnpaidInvoices { get; set; }
    public int PendingClaims { get; set; }
    public int PendingOrders { get; set; }
    public int PendingPrescriptions { get; set; }
    public decimal RevenueToday { get; set; }
}

public class PatientListViewModel
{
    public List<Patient> Patients { get; set; } = new();
    public string Search { get; set; } = "";
}

public class PatientDetailViewModel
{
    public Patient Patient { get; set; } = null!;
    public List<Allergy> Allergies { get; set; } = new();
    public List<MedicalHistory> MedicalHistories { get; set; } = new();
}

public class InsuranceClaimListViewModel
{
    public List<InsuranceClaim> Claims { get; set; } = new();
    public ClaimStatus? SelectedStatus { get; set; }
    public string Search { get; set; } = "";
    public InsuranceClaimStatsViewModel Stats { get; set; } = new();
}

public class InsuranceClaimStatsViewModel
{
    public int Pending { get; set; }
    public int Submitted { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public decimal TotalInsurancePaid { get; set; }
}

public class InsuranceClaimDetailViewModel
{
    public InsuranceClaim Claim { get; set; } = null!;
    public List<InsuranceClaimItem> Items { get; set; } = new();
}

public class DepartmentListViewModel
{
    public List<Department> Departments { get; set; } = new();
}

public class ServiceListViewModel
{
    public List<Service> Services { get; set; } = new();
}

public class TimeSlotListViewModel
{
    public List<TimeSlot> TimeSlots { get; set; } = new();
}

public class InsuranceConfigListViewModel
{
    public List<InsuranceConfig> Configs { get; set; } = new();
}

public class StaffListViewModel
{
    public List<Staff> Staffs { get; set; } = new();
}

public class StaffFormViewModel
{
    public Staff Staff { get; set; } = new();
    public Microsoft.AspNetCore.Mvc.Rendering.SelectList Departments { get; set; } = null!;
}

public class UserAccountRowViewModel
{
    public HisMvc.Data.AppUser User { get; set; } = null!;
    public string Roles { get; set; } = "";
}

public class UserListViewModel
{
    public List<UserAccountRowViewModel> Users { get; set; } = new();
}

public class UserFormViewModel
{
    public HisMvc.Data.AppUser? User { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? CurrentRole { get; set; }
    public Microsoft.AspNetCore.Mvc.Rendering.SelectList Staffs { get; set; } = null!;
}
