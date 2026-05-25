using System.Text;
using HisMvc.Areas.Admin.Models;
using HisMvc.Areas.Admin.Services;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.ADMIN)]
public class InsuranceClaimController : Controller
{
    private readonly AppDbContext _db;
    private readonly AdminViewService _views;
    private readonly InsuranceService _insurance;
    private readonly CurrentStaffService _staffService;

    public InsuranceClaimController(AppDbContext db, AdminViewService views, InsuranceService insurance, CurrentStaffService staffService)
    {
        _db = db;
        _views = views;
        _insurance = insurance;
        _staffService = staffService;
    }

    public async Task<IActionResult> Index(ClaimStatus? status, string? search)
    {
        var model = await _views.GetInsuranceClaimListAsync(status, search);
        return View(model);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var model = await _views.GetInsuranceClaimDetailAsync(id);
        if (model == null)
            return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id)
    {
        var claim = await _db.InsuranceClaims.FindAsync(id);
        if (claim == null || claim.Status != ClaimStatus.Pending)
        {
            TempData["Error"] = "Không thể nop hồ sơ nay!";
            return RedirectToAction(nameof(Index));
        }

        claim.Status = ClaimStatus.Submitted;
        claim.SubmittedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _insurance.ExportClaimXmlAsync(id);

        TempData["Success"] = $"Đã nop hồ sơ giam dinh {claim.ClaimCode}.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, decimal? approvedAmount, string? note)
    {
        var claim = await _db.InsuranceClaims.FindAsync(id);
        if (claim == null || claim.Status != ClaimStatus.Submitted)
        {
            TempData["Error"] = "Ho so chua o trạng thái cho duyet!";
            return RedirectToAction(nameof(Index));
        }

        var staffId = await _staffService.TryGetStaffIdAsync(User);
        claim.Status = ClaimStatus.Approved;
        claim.ApprovedAt = DateTime.UtcNow;
        claim.ApprovedBy = staffId;
        claim.Note = note;

        if (approvedAmount.HasValue && approvedAmount.Value < claim.InsuranceCovered)
        {
            claim.Status = ClaimStatus.PartiallyApproved;
            claim.InsuranceCovered = approvedAmount.Value;
            claim.PatientPayment = claim.TotalAmount - approvedAmount.Value;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã duyet hồ sơ giam dinh!";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        var claim = await _db.InsuranceClaims.FindAsync(id);
        if (claim == null || claim.Status != ClaimStatus.Submitted)
        {
            TempData["Error"] = "Ho so không o trạng thái cho duyet!";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "Vui lòng ghi ly do tu choi!";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var staffId = await _staffService.TryGetStaffIdAsync(User);
        claim.Status = ClaimStatus.Rejected;
        claim.ApprovedAt = DateTime.UtcNow;
        claim.ApprovedBy = staffId;
        claim.RejectReason = reason.Trim();
        claim.InsuranceCovered = 0;
        claim.PatientPayment = claim.TotalAmount;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã tu choi hồ sơ giam dinh.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> ExportXml(int id)
    {
        var xml = await _insurance.ExportClaimXmlAsync(id);
        if (string.IsNullOrEmpty(xml))
            return NotFound();

        var bytes = Encoding.UTF8.GetBytes(xml);
        var claim = await _db.InsuranceClaims.FindAsync(id);
        return File(bytes, "application/xml", $"{claim?.ClaimCode ?? "claim"}.xml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateForEncounter(int encounterId)
    {
        if (await _db.InsuranceClaims.AnyAsync(c => c.EncounterId == encounterId))
        {
            TempData["Error"] = "Lượt khám nay đã có hồ sơ giam dinh!";
            return RedirectToAction(nameof(Index));
        }

        var calc = await _insurance.CalculateInsuranceForEncounter(encounterId);
        if (!calc.IsValid)
        {
            TempData["Error"] = calc.ErrorMessage ?? "Bệnh nhân không có BHYT hop le!";
            return RedirectToAction(nameof(Index));
        }

        var claim = await _insurance.CreateInsuranceClaim(encounterId, calc);
        TempData["Success"] = $"Đã tao hồ sơ giam dinh {claim.ClaimCode}.";
        return RedirectToAction(nameof(Detail), new { id = claim.InsuranceClaimId });
    }
}
