using HisMvc.Areas.Admin.Models;
using HisMvc.Areas.Admin.Services;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.ADMIN)]
public class PatientController : Controller
{
    private readonly AppDbContext _db;
    private readonly AdminViewService _views;

    public PatientController(AppDbContext db, AdminViewService views)
    {
        _db = db;
        _views = views;
    }

    public async Task<IActionResult> Index(string search = "")
    {
        var model = await _views.GetPatientListAsync(search);
        return View(model);
    }

    public IActionResult Create() => View(new Patient());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Patient model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _db.Patients.AnyAsync(x => x.Phone == model.Phone))
        {
            ModelState.AddModelError(nameof(Patient.Phone), "So dien thoai da tồn tại trong he thong!");
            return View(model);
        }

        _db.Patients.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Thêm bệnh nhân thành công!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient == null)
            return NotFound();
        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Patient model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var patient = await _db.Patients.FindAsync(model.PatientId);
        if (patient == null)
            return NotFound();

        if (await _db.Patients.AnyAsync(x => x.Phone == model.Phone && x.PatientId != model.PatientId))
        {
            ModelState.AddModelError(nameof(Patient.Phone), "So dien thoai da duoc su dung boi bệnh nhân khac!");
            return View(model);
        }

        patient.FullName = model.FullName;
        patient.Phone = model.Phone;
        patient.Dob = model.Dob;
        patient.Gender = model.Gender;
        patient.IdentityNumber = model.IdentityNumber;
        patient.Address = model.Address;
        patient.InsuranceNumber = model.InsuranceNumber;
        patient.InsuranceExpiry = model.InsuranceExpiry;
        patient.InsuranceType = model.InsuranceType;
        patient.InsuranceCoveragePercent = model.InsuranceCoveragePercent;
        patient.InsuranceHospital = model.InsuranceHospital;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Cập nhật thông tin bệnh nhân thành công!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await _views.GetPatientDetailAsync(id);
        if (model == null)
            return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAllergy(int patientId, string allergen, string? reaction, AllergySeverity severity, string? note)
    {
        var patient = await _db.Patients.FindAsync(patientId);
        if (patient == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(allergen))
        {
            TempData["Error"] = "Vui lòng nhập chất gây di ung!";
            return RedirectToAction(nameof(Details), new { id = patientId });
        }

        _db.Allergies.Add(new Allergy
        {
            PatientId = patientId,
            Allergen = allergen.Trim(),
            Reaction = reaction?.Trim(),
            Severity = severity,
            Note = note?.Trim(),
            IdentifiedDate = DateTime.UtcNow,
            IsActive = true
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = "Thêm dị ứng thành công!";
        return RedirectToAction(nameof(Details), new { id = patientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMedicalHistory(int patientId, string condition, DateTime diagnosedDate, string? treatment, string? note)
    {
        var patient = await _db.Patients.FindAsync(patientId);
        if (patient == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(condition))
        {
            TempData["Error"] = "Vui lòng nhập tên bệnh / tinh trang!";
            return RedirectToAction(nameof(Details), new { id = patientId });
        }

        _db.MedicalHistories.Add(new MedicalHistory
        {
            PatientId = patientId,
            Condition = condition.Trim(),
            DiagnosedDate = diagnosedDate,
            Treatment = treatment?.Trim(),
            Note = note?.Trim(),
            IsActive = true
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = "Thêm tien su benh thành công!";
        return RedirectToAction(nameof(Details), new { id = patientId });
    }
}
