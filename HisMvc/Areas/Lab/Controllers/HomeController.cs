using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Lab.Controllers;

[Area("Lab")]
[Authorize(Roles = AppRoles.LAB_TECH + "," + AppRoles.ADMIN)]
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    // Danh sách chỉ định chờ xử lý (với filter)
    public async Task<IActionResult> Index(string serviceType = "", DateOnly? date = null, int? departmentId = null)
    {
        var query = _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Encounter)!.ThenInclude(e => e.Patient)
            .Include(o => o.Encounter)!.ThenInclude(e => e.Doctor)!.ThenInclude(d => d.Department)
            .Include(o => o.OrderResult)
            .AsQueryable();

        // Filter theo Service Type (LAB hoặc IMAGING)
        if (!string.IsNullOrEmpty(serviceType))
        {
            query = query.Where(o => o.Service!.Type == serviceType);
        }
        else
        {
            // Mặc định chỉ hiển thị LAB và IMAGING
            query = query.Where(o => o.Service!.Type == "LAB" || o.Service!.Type == "IMAGING");
        }

        // Filter theo ngày
        if (date.HasValue)
        {
            query = query.Where(o => DateOnly.FromDateTime(o.OrderedAt) == date.Value);
        }

        // Filter theo khoa
        if (departmentId.HasValue)
        {
            query = query.Where(o => o.Encounter!.Doctor!.DepartmentId == departmentId.Value);
        }

        // Chỉ hiển thị Requested (chờ xử lý)
        query = query.Where(o => o.Status == OrderStatus.Requested);

        var orders = await query
            .OrderBy(o => o.OrderedAt)
            .ToListAsync();

        ViewBag.ServiceType = serviceType;
        ViewBag.Date = date;
        ViewBag.DepartmentId = departmentId;
        ViewBag.Departments = await _db.Departments.ToListAsync();

        return View(orders);
    }

    // Danh sách đã có kết quả (History)
    public async Task<IActionResult> History(string serviceType = "", DateOnly? date = null)
    {
        var query = _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Encounter)!.ThenInclude(e => e.Patient)
            .Include(o => o.Encounter)!.ThenInclude(e => e.Doctor)
            .Include(o => o.OrderResult)
            .AsQueryable();

        // Filter theo Service Type
        if (!string.IsNullOrEmpty(serviceType))
        {
            query = query.Where(o => o.Service!.Type == serviceType);
        }
        else
        {
            query = query.Where(o => o.Service!.Type == "LAB" || o.Service!.Type == "IMAGING");
        }

        // Filter theo ngày
        if (date.HasValue)
        {
            query = query.Where(o => DateOnly.FromDateTime(o.OrderedAt) == date.Value);
        }
        else
        {
            // Mặc định: hôm nay
            var today = DateOnly.FromDateTime(DateTime.Today);
            query = query.Where(o => DateOnly.FromDateTime(o.OrderedAt) == today);
        }

        // Chỉ hiển thị Resulted (đã có kết quả)
        query = query.Where(o => o.Status == OrderStatus.Resulted);

        var orders = await query
            .OrderByDescending(o => o.OrderResult!.ResultedAt)
            .ToListAsync();

        ViewBag.ServiceType = serviceType;
        ViewBag.Date = date ?? DateOnly.FromDateTime(DateTime.Today);

        return View(orders);
    }

    // Màn hình nhập kết quả
    public async Task<IActionResult> Result(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Encounter)!.ThenInclude(e => e.Patient)
            .Include(o => o.Encounter)!.ThenInclude(e => e.Doctor)
            .Include(o => o.OrderResult)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null)
        {
            TempData["Error"] = "Khong tim thay chi dinh!";
            return RedirectToAction(nameof(Index));
        }

        return View(order);
    }

    // Lưu kết quả
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(int orderId, string resultText)
    {
        var order = await _db.Orders
            .Include(o => o.OrderResult)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            TempData["Error"] = "Khong tim thay chi dinh!";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(resultText))
        {
            TempData["Error"] = "Vui long nhap ket qua!";
            return RedirectToAction(nameof(Result), new { id = orderId });
        }

        var existing = order.OrderResult;
        if (existing == null)
        {
            // Tạo mới kết quả
            existing = new OrderResult
            {
                OrderId = orderId,
                ResultText = resultText.Trim(),
                ResultedBy = User.Identity?.Name ?? "lab",
                ResultedAt = DateTime.UtcNow
            };
            _db.OrderResults.Add(existing);
        }
        else
        {
            // Cập nhật kết quả
            existing.ResultText = resultText.Trim();
            existing.ResultedBy = User.Identity?.Name ?? "lab";
            existing.ResultedAt = DateTime.UtcNow;
        }

        // Cập nhật trạng thái Order
        order.Status = OrderStatus.Resulted;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Da luu ket qua thanh cong!";
        return RedirectToAction(nameof(Index));
    }

    // Xóa/Hủy kết quả (chuyển về Requested)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearResult(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.OrderResult)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            TempData["Error"] = "Khong tim thay chi dinh!";
            return RedirectToAction(nameof(Index));
        }

        if (order.OrderResult != null)
        {
            _db.OrderResults.Remove(order.OrderResult);
        }

        order.Status = OrderStatus.Requested;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Da xoa ket qua!";
        return RedirectToAction(nameof(Result), new { id = orderId });
    }
}
