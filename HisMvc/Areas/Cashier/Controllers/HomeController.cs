using HisMvc.Areas.Cashier.Models;
using HisMvc.Areas.Cashier.Services;
using HisMvc.Models;
using HisMvc.Services;
using HisMvc.Services.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HisMvc.Areas.Cashier.Controllers;

[Area("Cashier")]
[Authorize(Roles = AppRoles.CASHIER + "," + AppRoles.ADMIN)]
public class HomeController : Controller
{
    private readonly CashierViewService _views;
    private readonly CurrentStaffService _staff;
    private readonly OutpatientWorkflowService _workflow;

    public HomeController(CashierViewService views, CurrentStaffService staff, OutpatientWorkflowService workflow)
    {
        _views = views;
        _staff = staff;
        _workflow = workflow;
    }

    // GET — hien thi dashboard
    public async Task<IActionResult> Dashboard()
    {
        var model = await _views.BuildDashboardAsync();
        return View(model);
    }

    // GET — danh sach hóa đơn
    public async Task<IActionResult> Index(string status = "", DateOnly? date = null, string type = "")
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.Today);
        var model = await _views.GetInvoiceListAsync(status, type, d);
        return View(model);
    }

    // GET — cho thu chi phí cuoi (buoc 8)
    public async Task<IActionResult> Pending(DateOnly? date = null)
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.Today);
        var model = await _views.GetPendingPaymentsAsync(d);
        return View(model);
    }

    // URL cu — chuyen ve Pending
    public IActionResult Create(int encounterId) => RedirectToAction(nameof(Pending));

    // GET — chi tiet hóa đơn
    public async Task<IActionResult> Detail(int id)
    {
        var model = await _views.GetInvoiceDetailAsync(id);
        if (model == null)
        {
            TempData["Error"] = "Không tìm thay hóa đơn!";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    // POST — thu tien (buoc 2 hoặc 8)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int id)
    {
        var staffId = await _staff.GetCurrentStaffIdAsync(User);
        var userName = User.Identity?.Name ?? "cashier";

        var result = await _workflow.PayInvoiceAsync(id, userName, staffId);

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(result.Success ? nameof(Detail) : nameof(Index), new { id });
    }
}
