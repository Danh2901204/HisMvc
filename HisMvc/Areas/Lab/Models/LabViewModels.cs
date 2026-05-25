using HisMvc.Entities;
using HisMvc.Models;

namespace HisMvc.Areas.Lab.Models;

public class LabDashboardViewModel
{
    public LabKpiViewModel Kpi { get; set; } = new();
    public List<Order> PendingOrders { get; set; } = new();
    public List<DashboardActivity> Activities { get; set; } = new();
}

public class LabKpiViewModel
{
    public int LabPending { get; set; }
    public int ImagingPending { get; set; }
    public int DoneToday { get; set; }
    public int OrderToday { get; set; }
}

public class OrderListViewModel
{
    public List<Order> Orders { get; set; } = new();
    public string ServiceType { get; set; } = "";
    public DateOnly? SelectedDate { get; set; }
    public int? DepartmentId { get; set; }
    public List<Department> Departments { get; set; } = new();
    public string Title { get; set; } = "Danh sách chi dinh CLS";
}

public class LabResultViewModel
{
    public Order Order { get; set; } = null!;
}
