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
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly InpatientViewService _views;
    private readonly CurrentStaffService _staffService;

    public HomeController(AppDbContext db, InpatientViewService views, CurrentStaffService staffService)
    {
        _db = db;
        _views = views;
        _staffService = staffService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var model = await _views.BuildDashboardAsync();
        return View(model);
    }

    public async Task<IActionResult> Index(int? wardId, AdmissionStatus? status)
    {
        var model = await _views.GetAdmissionListAsync(wardId, status);
        return View(model);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var model = await _views.GetAdmissionDetailAsync(id);
        if (model == null)
            return NotFound();
        return View(model);
    }

    public async Task<IActionResult> Admit(int? patientId)
    {
        var model = await _views.GetAdmitFormAsync(patientId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Admit(int patientId, int bedId, int attendingDoctorId,
        string admissionReason, string? initialDiagnosis, string? icd10Admission)
    {
        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var bed = await _db.Beds.FindAsync(bedId);
            if (bed == null || bed.Status != BedStatus.Empty)
            {
                TempData["Error"] = "Gi??ng b?nh không kh? dung!";
                return RedirectToAction(nameof(Admit), new { patientId });
            }

            var admissionCode = $"ADM{DateTime.UtcNow:yyyyMMddHHmmss}";

            var admission = new Admission
            {
                AdmissionCode = admissionCode,
                PatientId = patientId,
                BedId = bedId,
                AttendingDoctorId = attendingDoctorId,
                AdmittedAt = DateTime.UtcNow,
                Status = AdmissionStatus.Active,
                AdmissionReason = admissionReason,
                InitialDiagnosis = initialDiagnosis,
                Icd10Admission = string.IsNullOrWhiteSpace(icd10Admission) ? null : icd10Admission.Trim().ToUpperInvariant()
            };

            _db.Admissions.Add(admission);
            bed.Status = BedStatus.Occupied;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = $"?ã nh?p vi?n thành công! Mã: {admissionCode}";
            return RedirectToAction(nameof(Detail), new { id = admission.AdmissionId });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            TempData["Error"] = $"L?i: {ex.Message}";
            return RedirectToAction(nameof(Admit), new { patientId });
        }
    }

    public async Task<IActionResult> Discharge(int id)
    {
        var admission = await _db.Admissions
            .Include(a => a.Patient)
            .Include(a => a.Bed).ThenInclude(b => b!.Ward)
            .Include(a => a.AttendingDoctor)
            .FirstOrDefaultAsync(a => a.AdmissionId == id);

        if (admission == null || admission.Status != AdmissionStatus.Active)
            return NotFound();

        return View(admission);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Discharge(int id, string dischargeSummary, string? dischargeInstructions)
    {
        var admission = await _db.Admissions
            .Include(a => a.Bed)
            .FirstOrDefaultAsync(a => a.AdmissionId == id);

        if (admission == null || admission.Status != AdmissionStatus.Active)
        {
            TempData["Error"] = "Không tìm thay h? s? ho?c ?ã xu?t vi?n!";
            return RedirectToAction(nameof(Index));
        }

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var staffId = await _staffService.TryGetStaffIdAsync(User);
            admission.Status = AdmissionStatus.Discharged;
            admission.DischargedAt = DateTime.UtcNow;
            admission.DischargeSummary = dischargeSummary;
            admission.DischargeInstructions = dischargeInstructions;
            admission.DischargedBy = staffId;

            if (admission.Bed != null)
                admission.Bed.Status = BedStatus.Cleaning;

            var rep = await _db.Encounters
                .FirstOrDefaultAsync(e => e.PatientId == admission.PatientId && e.Conclusion == $"Noi tru: {admission.AdmissionCode}");
            if (rep != null)
            {
                rep.Status = EncounterStatus.Completed;
                rep.EndAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = "?ã xu?t vi?n thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            TempData["Error"] = $"L?i: {ex.Message}";
            return RedirectToAction(nameof(Discharge), new { id });
        }
    }

    public async Task<IActionResult> AddVitalSign(int admissionId)
    {
        var model = await _views.GetVitalSignFormAsync(admissionId);
        if (model == null)
            return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVitalSign(int admissionId, decimal? temperature, int? heartRate,
        int? respiratoryRate, int? bpSystolic, int? bpDiastolic, decimal? oxygenSat, decimal? weight, decimal? height, string? note)
    {
        try
        {
            var admission = await _db.Admissions.FindAsync(admissionId);
            var staffId = await _staffService.GetStaffIdAsync(User, admission?.AttendingDoctorId);

            var vitalSign = new VitalSign
            {
                AdmissionId = admissionId,
                RecordedAt = DateTime.UtcNow,
                RecordedBy = staffId,
                Temperature = temperature,
                HeartRate = heartRate,
                RespiratoryRate = respiratoryRate,
                BloodPressureSystolic = bpSystolic,
                BloodPressureDiastolic = bpDiastolic,
                OxygenSaturation = oxygenSat,
                Weight = weight,
                Height = height,
                Note = note
            };

            _db.VitalSigns.Add(vitalSign);
            await _db.SaveChangesAsync();

            TempData["Success"] = "?ã ghi nh?n sinh hi?u!";
            return RedirectToAction(nameof(Detail), new { id = admissionId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"L?i: {ex.Message}";
            return RedirectToAction(nameof(AddVitalSign), new { admissionId });
        }
    }
}
