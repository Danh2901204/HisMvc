using HisMvc.Areas.Lab.Models;
using HisMvc.Areas.Lab.Services;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Services;
using HisMvc.Services.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Lab.Controllers;

[Area("Lab")]
[Authorize(Roles = AppRoles.LAB_TECH + "," + AppRoles.ADMIN)]
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly LabViewService _views;
    private readonly CurrentStaffService _staffService;
    private readonly OutpatientWorkflowService _workflow;

    public HomeController(AppDbContext db, LabViewService views, CurrentStaffService staffService, OutpatientWorkflowService workflow)
    {
        _db = db;
        _views = views;
        _staffService = staffService;
        _workflow = workflow;
    }

    public async Task<IActionResult> Dashboard()
    {
        var model = await _views.BuildDashboardAsync();
        return View(model);
    }

    public async Task<IActionResult> Index(string serviceType = "", DateOnly? date = null, int? departmentId = null)
    {
        var model = await _views.GetPendingOrdersAsync(serviceType, date, departmentId);
        return View(model);
    }

    public async Task<IActionResult> History(string serviceType = "", DateOnly? date = null)
    {
        var model = await _views.GetOrderHistoryAsync(serviceType, date);
        return View(model);
    }

    public async Task<IActionResult> Result(int id)
    {
        var model = await _views.GetResultFormAsync(id);
        if (model == null)
        {
            TempData["Error"] = "Không tìm thay chi dinh!";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartOrder(int orderId)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) { TempData["Error"] = "Không tìm thay chi dinh!"; return RedirectToAction(nameof(Index)); }
        if (order.Status != OrderStatus.Requested) { TempData["Error"] = "Chỉ định không o trạng thái cho."; return RedirectToAction(nameof(Index)); }

        order.Status = OrderStatus.InProgress;
        order.StartedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Bat dau thuc hien chi dinh.";
        return RedirectToAction(nameof(Result), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(int orderId, string resultText)
    {
        var staffId = await _staffService.TryGetStaffIdAsync(User);
        var userName = User.Identity?.Name ?? "lab";

        var result = await _workflow.SaveLabResultAsync(orderId, resultText, userName, staffId);

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(result.Success ? nameof(Index) : nameof(Result), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearResult(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.OrderResult)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            TempData["Error"] = "Không tìm thay chi dinh!";
            return RedirectToAction(nameof(Index));
        }

        if (order.OrderResult != null)
            _db.OrderResults.Remove(order.OrderResult);

        order.Status = OrderStatus.Requested;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã xóa ket qua!";
        return RedirectToAction(nameof(Result), new { id = orderId });
    }
}
