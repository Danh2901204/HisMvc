using HisMvc.Entities;
using HisMvc.Models;

namespace HisMvc.Areas.Pharmacy.Models;

public class PharmacyDashboardViewModel
{
    public PharmacyKpiViewModel Kpi { get; set; } = new();
    public List<Prescription> PendingPrescriptions { get; set; } = new();
    public List<MedicineBatch> LowStockBatches { get; set; } = new();
    public List<DashboardActivity> Activities { get; set; } = new();
}

public class PharmacyKpiViewModel
{
    public int Pending { get; set; }
    public int DispensedToday { get; set; }
    public int LowStock { get; set; }
    public int TotalMedicines { get; set; }
}

public class PrescriptionListViewModel
{
    public List<Prescription> Prescriptions { get; set; } = new();
    public DateOnly SelectedDate { get; set; }
    public PrescriptionStatus? SelectedStatus { get; set; }
}

public class PrescriptionDetailViewModel
{
    public Prescription Prescription { get; set; } = null!;
    public Dictionary<int, int> Stocks { get; set; } = new();
    public bool CanDispense { get; set; }
}

public class DispenseViewModel
{
    public Prescription Prescription { get; set; } = null!;
    public Dictionary<int, List<MedicineBatch>> Batches { get; set; } = new();
}

public class DispenseHistoryViewModel
{
    public List<PharmacyDispense> Dispenses { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class MedicineListViewModel
{
    public List<Medicine> Medicines { get; set; } = new();
    public string? Search { get; set; }
    public bool? Active { get; set; }
}
