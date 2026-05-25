using HisMvc.Areas.Inpatient.Models;
using HisMvc.Areas.Inpatient.Services;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Inpatient.Controllers;

[Area("Inpatient")]
[Authorize(Roles = AppRoles.DOCTOR + "," + AppRoles.ADMIN)]
public class SurgeryController : Controller
{
    private readonly AppDbContext _db;
    private readonly InpatientViewService _views;
    private readonly CurrentStaffService _staffService;

    public SurgeryController(AppDbContext db, InpatientViewService views, CurrentStaffService staffService)
    {
        _db = db;
        _views = views;
        _staffService = staffService;
    }

    public async Task<IActionResult> Index(int admissionId)
    {
        var model = await _views.GetSurgeryListAsync(admissionId);
        if (model == null)
            return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int admissionId,
        string surgeryType,
        string? description,
        DateTime scheduledAt,
        int surgeonId,
        int? anesthesiologistId,
        string? operatingRoom)
    {
        var admission = await _db.Admissions.FindAsync(admissionId);
        if (admission == null || admission.Status != AdmissionStatus.Active)
        {
            TempData["Error"] = "Không tìm thay hồ sơ noi tru hoặc đã xuất viện!";
            return RedirectToAction(nameof(Index), new { admissionId });
        }

        if (string.IsNullOrWhiteSpace(surgeryType))
        {
            TempData["Error"] = "Vui lòng nhập ten phẩu thuật / thủ thuật!";
            return RedirectToAction(nameof(Index), new { admissionId });
        }

        var surgery = new Surgery
        {
            SurgeryCode = $"PT{DateTime.UtcNow:yyyyMMddHHmmss}",
            AdmissionId = admissionId,
            SurgeryType = surgeryType.Trim(),
            Description = description?.Trim(),
            ScheduledAt = scheduledAt == default ? DateTime.UtcNow.AddHours(1) : scheduledAt,
            SurgeonId = surgeonId,
            AnesthesiologistId = anesthesiologistId,
            OperatingRoom = operatingRoom?.Trim(),
            Status = SurgeryStatus.Scheduled
        };

        _db.Surgeries.Add(surgery);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã lap phieu phẩu thuật {surgery.SurgeryCode}.";
        return RedirectToAction(nameof(Index), new { admissionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int id)
    {
        var s = await _db.Surgeries.FindAsync(id);
        if (s == null || s.Status != SurgeryStatus.Scheduled)
        {
            TempData["Error"] = "Không thể bat dau phẩu thuật!";
            return RedirectToAction(nameof(Index), new { admissionId = s?.AdmissionId ?? 0 });
        }

        s.Status = SurgeryStatus.InProgress;
        s.StartedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã bat dau phẩu thuật.";
        return RedirectToAction(nameof(Index), new { admissionId = s.AdmissionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, string? operativeNotes, string? postOpInstructions, string? complications)
    {
        var s = await _db.Surgeries.FindAsync(id);
        if (s == null || s.Status != SurgeryStatus.InProgress)
        {
            TempData["Error"] = "Không thể kết thúc phẩu thuật!";
            return RedirectToAction(nameof(Index), new { admissionId = s?.AdmissionId ?? 0 });
        }

        if (string.IsNullOrWhiteSpace(operativeNotes))
        {
            TempData["Error"] = "Vui lòng ghi bien ban phẩu thuật (TT 56/2017)!";
            return RedirectToAction(nameof(Index), new { admissionId = s.AdmissionId });
        }

        s.Status = SurgeryStatus.Completed;
        s.EndedAt = DateTime.UtcNow;
        s.OperativeNotes = operativeNotes.Trim();
        s.PostOpInstructions = postOpInstructions?.Trim();
        s.Complications = complications?.Trim();
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã hoan tat phẩu thuật va luu bien ban.";
        return RedirectToAction(nameof(Index), new { admissionId = s.AdmissionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason, bool postpone = false)
    {
        var s = await _db.Surgeries.FindAsync(id);
        if (s == null || s.Status == SurgeryStatus.Completed)
        {
            TempData["Error"] = "Không thể huy phẩu thuật nay!";
            return RedirectToAction(nameof(Index), new { admissionId = s?.AdmissionId ?? 0 });
        }

        s.Status = postpone ? SurgeryStatus.Postponed : SurgeryStatus.Cancelled;
        s.Complications = $"{(postpone ? "Hoan" : "Hủy")}: {reason}";
        await _db.SaveChangesAsync();
        TempData["Success"] = postpone ? "Đã hoan phẩu thuật." : "Đã huy phẩu thuật.";
        return RedirectToAction(nameof(Index), new { admissionId = s.AdmissionId });
    }
}
