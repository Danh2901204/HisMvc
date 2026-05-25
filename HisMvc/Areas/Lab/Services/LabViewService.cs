using HisMvc.Areas.Lab.Models;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Lab.Services;

public class LabViewService
{
    private readonly AppDbContext _db;

    public LabViewService(AppDbContext db) => _db = db;

    public async Task<LabDashboardViewModel> BuildDashboardAsync()
    {
        var todayStart = DateTime.Today;
        var todayEnd = todayStart.AddDays(1);

        var vm = new LabDashboardViewModel
        {
            Kpi = new LabKpiViewModel
            {
                LabPending = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Requested && o.Service!.Type == "LAB"),
                ImagingPending = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Requested && o.Service!.Type == "IMAGING"),
                DoneToday = await _db.OrderResults.CountAsync(r => r.ResultedAt >= todayStart && r.ResultedAt < todayEnd),
                OrderToday = await _db.Orders.CountAsync(o => o.OrderedAt >= todayStart && o.OrderedAt < todayEnd)
            },
            PendingOrders = await _db.Orders
                .Include(o => o.Service)
                .Include(o => o.Encounter)!.ThenInclude(e => e!.Patient)
                .Include(o => o.Encounter)!.ThenInclude(e => e!.Doctor)
                .Where(o => o.Status == OrderStatus.Requested &&
                            (o.Service!.Type == "LAB" || o.Service!.Type == "IMAGING"))
                .OrderBy(o => o.OrderedAt)
                .Take(15)
                .ToListAsync()
        };

        vm.Activities = await BuildActivitiesAsync(todayStart);
        return vm;
    }

    public async Task<OrderListViewModel> GetPendingOrdersAsync(string serviceType, DateOnly? date, int? departmentId)
    {
        var query = _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Encounter)!.ThenInclude(e => e!.Patient)
            .Include(o => o.Encounter)!.ThenInclude(e => e!.Doctor)!.ThenInclude(d => d!.Department)
            .Include(o => o.OrderResult)
            .AsQueryable();

        if (!string.IsNullOrEmpty(serviceType))
            query = query.Where(o => o.Service!.Type == serviceType);
        else
            query = query.Where(o => o.Service!.Type == "LAB" || o.Service!.Type == "IMAGING");

        if (date.HasValue)
            query = query.Where(o => DateOnly.FromDateTime(o.OrderedAt) == date.Value);

        if (departmentId.HasValue)
            query = query.Where(o => o.Encounter!.Doctor!.DepartmentId == departmentId.Value);

        query = query.Where(o => o.Status == OrderStatus.Requested || o.Status == OrderStatus.InProgress);

        return new OrderListViewModel
        {
            Orders = await query.OrderBy(o => o.OrderedAt).ToListAsync(),
            ServiceType = serviceType,
            SelectedDate = date,
            DepartmentId = departmentId,
            Departments = await _db.Departments.ToListAsync()
        };
    }

    public async Task<OrderListViewModel> GetOrderHistoryAsync(string serviceType, DateOnly? date)
    {
        var query = _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Encounter)!.ThenInclude(e => e!.Patient)
            .Include(o => o.Encounter)!.ThenInclude(e => e!.Doctor)
            .Include(o => o.OrderResult)
            .AsQueryable();

        if (!string.IsNullOrEmpty(serviceType))
            query = query.Where(o => o.Service!.Type == serviceType);
        else
            query = query.Where(o => o.Service!.Type == "LAB" || o.Service!.Type == "IMAGING");

        var targetDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        query = query.Where(o => DateOnly.FromDateTime(o.OrderedAt) == targetDate);
        query = query.Where(o => o.Status == OrderStatus.Resulted);

        return new OrderListViewModel
        {
            Orders = await query.OrderByDescending(o => o.OrderResult!.ResultedAt).ToListAsync(),
            ServiceType = serviceType,
            SelectedDate = targetDate,
            Title = "Lịch sử ket qua CLS"
        };
    }

    public async Task<LabResultViewModel?> GetResultFormAsync(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Encounter)!.ThenInclude(e => e!.Patient)
            .Include(o => o.Encounter)!.ThenInclude(e => e!.Doctor)
            .Include(o => o.OrderResult)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null) return null;

        return new LabResultViewModel { Order = order };
    }

    private async Task<List<DashboardActivity>> BuildActivitiesAsync(DateTime todayStart)
    {
        var activities = new List<DashboardActivity>();
        var newOrders = await _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Encounter)!.ThenInclude(e => e!.Patient)
            .Where(o => (o.Service!.Type == "LAB" || o.Service!.Type == "IMAGING") &&
                        o.OrderedAt >= todayStart.AddDays(-1))
            .OrderByDescending(o => o.OrderedAt).Take(10).ToListAsync();

        foreach (var o in newOrders)
        {
            activities.Add(new DashboardActivity
            {
                At = o.OrderedAt,
                Icon = o.Service!.Type == "IMAGING" ? "bi-camera-fill" : "bi-flask",
                Title = $"CD {o.Service.Name}",
                Detail = $"BN {o.Encounter?.Patient?.FullName}",
                Url = $"/Lab/Home/Result/{o.OrderId}",
                Tag = o.Status == OrderStatus.Requested ? "Cho" : "Đã co KQ",
                Priority = o.Status == OrderStatus.Requested ? "warning" : "success"
            });
        }

        return activities.OrderByDescending(x => x.At).Take(20).ToList();
    }
}
