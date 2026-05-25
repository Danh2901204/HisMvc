using HisMvc.Entities;
using HisMvc.Models;

namespace HisMvc.Areas.Inpatient.Models;

public class InpatientDashboardViewModel
{
    public InpatientKpiViewModel Kpi { get; set; } = new();
    public List<Admission> ActiveAdmissions { get; set; } = new();
    public List<DashboardActivity> Activities { get; set; } = new();
}

public class InpatientKpiViewModel
{
    public int ActiveAdmissions { get; set; }
    public int AdmittedToday { get; set; }
    public int DischargedToday { get; set; }
    public int PendingOrders { get; set; }
    public int ActiveSurgeries { get; set; }
}

public class AdmissionListViewModel
{
    public List<Admission> Admissions { get; set; } = new();
    public List<Ward> Wards { get; set; } = new();
    public int? SelectedWardId { get; set; }
    public AdmissionStatus? SelectedStatus { get; set; }
}

public class AdmissionDetailViewModel
{
    public Admission Admission { get; set; } = null!;
    public List<VitalSign> VitalSigns { get; set; } = new();
    public List<Prescription> Prescriptions { get; set; } = new();
    public List<Allergy> Allergies { get; set; } = new();
    public List<MedicalHistory> MedicalHistories { get; set; } = new();
    public List<Surgery> Surgeries { get; set; } = new();
}

public class AdmitFormViewModel
{
    public List<Bed> AvailableBeds { get; set; } = new();
    public List<Staff> Doctors { get; set; } = new();
    public Patient? Patient { get; set; }
}

public class VitalSignFormViewModel
{
    public Admission Admission { get; set; } = null!;
}

public class MedicalOrderListViewModel
{
    public Admission Admission { get; set; } = null!;
    public List<MedicalOrder> Orders { get; set; } = new();
    public List<Medicine> Medicines { get; set; } = new();
    public List<Service> Services { get; set; } = new();
}

public class SurgeryListViewModel
{
    public Admission Admission { get; set; } = null!;
    public List<Surgery> Surgeries { get; set; } = new();
    public List<Staff> Doctors { get; set; } = new();
}

public class WardListViewModel
{
    public List<Ward> Wards { get; set; } = new();
    public Dictionary<int, int> BedCounts { get; set; } = new();
}

public class WardFormViewModel
{
    public Ward Ward { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
}

public class BedMapViewModel
{
    public Ward Ward { get; set; } = null!;
    public List<Bed> Beds { get; set; } = new();
    public Dictionary<int, Admission> AdmissionsByBed { get; set; } = new();
}

public class AddBedFormViewModel
{
    public Bed Bed { get; set; } = null!;
    public Ward Ward { get; set; } = null!;
}
