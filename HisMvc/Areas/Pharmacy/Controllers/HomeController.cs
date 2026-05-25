using HisMvc.Areas.Pharmacy.Models;
using HisMvc.Areas.Pharmacy.Services;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Services;
using HisMvc.Services.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HisMvc.Areas.Pharmacy.Controllers;

[Area("Pharmacy")]
[Authorize(Roles = AppRoles.PHARMACIST + "," + AppRoles.ADMIN)]
public class HomeController : Controller
{
    private readonly PharmacyViewService _views;
    private readonly CurrentStaffService _staffService;
    private readonly OutpatientWorkflowService _workflow;

    public HomeController(PharmacyViewService views, CurrentStaffService staffService, OutpatientWorkflowService workflow)
    {
        _views = views;
        _staffService = staffService;
        _workflow = workflow;
    }

    public async Task<IActionResult> Dashboard()
    {
        var model = await _views.BuildDashboardAsync();
        return View(model);
    }

    public async Task<IActionResult> Index(DateTime? date, PrescriptionStatus? status)
    {
        var model = await _views.GetPrescriptionListAsync(date, status);
        return View(model);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var model = await _views.GetPrescriptionDetailAsync(id);
        if (model == null)
            return NotFound();
        return View(model);
    }

    public async Task<IActionResult> Dispense(int id)
    {
        var model = await _views.GetDispenseFormAsync(id);
        if (model == null)
        {
            TempData["Error"] = "BN chua thanh toán chi phí cuoi tai Thu ngan. Không thể cấp phát thuốc.";
            return RedirectToAction(nameof(Detail), new { id });
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dispense(int id, string? note)
    {
        int staffId;
        try
        {
            staffId = await _staffService.GetStaffIdAsync(User);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Detail), new { id });
        }

        var result = await _workflow.DispensePrescriptionAsync(id, staffId, note);

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(result.Success ? nameof(Index) : nameof(Detail), new { id });
    }

    public async Task<IActionResult> History(DateTime? fromDate, DateTime? toDate)
    {
        var model = await _views.GetDispenseHistoryAsync(fromDate, toDate);
        return View(model);
    }
}
