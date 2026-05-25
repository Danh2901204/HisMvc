using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Inpatient.Controllers;

[Area("Inpatient")]
[Authorize(Roles = AppRoles.DOCTOR + "," + AppRoles.ADMIN)]
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    // Dashboard - Danh sach benh nhan noi tru
    public async Task<IActionResult> Index(int? wardId, AdmissionStatus? status)
    {
        var query = _db.Admissions
            .Include(a => a.Patient)
            .Include(a => a.Bed).ThenInclude(b => b!.Ward)
            .Include(a => a.AttendingDoctor)
            .AsQueryable();

        if (wardId.HasValue)
        {
            query = query.Where(a => a.Bed!.WardId == wardId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }
        else
        {
            // Mac dinh chi hien Active
            query = query.Where(a => a.Status == AdmissionStatus.Active);
        }

        var admissions = await query
            .OrderByDescending(a => a.AdmittedAt)
            .ToListAsync();

        // Lay danh sach ward cho filter
        var wards = await _db.Wards
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync();

        ViewBag.Wards = wards;
        ViewBag.SelectedWardId = wardId;
        ViewBag.SelectedStatus = status;

        return View(admissions);
    }

    // Chi tiet ho so nhap vien
    public async Task<IActionResult> Detail(int id)
    {
        var admission = await _db.Admissions
            .Include(a => a.Patient)
            .Include(a => a.Bed).ThenInclude(b => b!.Ward)
            .Include(a => a.AttendingDoctor)
            .Include(a => a.MedicalOrders).ThenInclude(o => o.OrderedByStaff)
            .FirstOrDefaultAsync(a => a.AdmissionId == id);

        if (admission == null)
        {
            return NotFound();
        }

        // Lay sinh hieu gan nhat
        var latestVitalSigns = await _db.VitalSigns
            .Where(v => v.AdmissionId == id)
            .OrderByDescending(v => v.RecordedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.VitalSigns = latestVitalSigns;

        // Lay don thuoc (neu co)
        var prescriptions = await _db.Prescriptions
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .Where(p => _db.MedicalOrders
                .Where(mo => mo.AdmissionId == id && mo.PrescriptionId != null)
                .Select(mo => mo.PrescriptionId)
                .Contains(p.PrescriptionId))
            .ToListAsync();

        ViewBag.Prescriptions = prescriptions;

        return View(admission);
    }

    // Nhap vien - GET
    public async Task<IActionResult> Admit(int? patientId)
    {
        // Lay danh sach giuong trong
        var availableBeds = await _db.Beds
            .Include(b => b.Ward)
            .Where(b => b.IsActive && b.Status == BedStatus.Empty)
            .OrderBy(b => b.Ward!.Name).ThenBy(b => b.BedNumber)
            .ToListAsync();

        ViewBag.AvailableBeds = availableBeds;

        // Lay danh sach bac si
        var doctors = await _db.Staffs
            .Where(s => s.StaffType == "DOCTOR")
            .OrderBy(s => s.FullName)
            .ToListAsync();

        ViewBag.Doctors = doctors;

        if (patientId.HasValue)
        {
            var patient = await _db.Patients.FindAsync(patientId.Value);
            ViewBag.Patient = patient;
        }

        return View();
    }

    // Nh?p vi?n - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Admit(int patientId, int bedId, int attendingDoctorId, string admissionReason, string? initialDiagnosis)
    {
        try
        {
            // Ki?m tra gi??ng còn tr?ng không
            var bed = await _db.Beds.FindAsync(bedId);
            if (bed == null || bed.Status != BedStatus.Empty)
            {
                TempData["Error"] = "Gi??ng b?nh không kh? d?ng!";
                return RedirectToAction(nameof(Admit), new { patientId });
            }

            // T?o mã nh?p vi?n
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
                InitialDiagnosis = initialDiagnosis
            };

            _db.Admissions.Add(admission);

            // C?p nh?t tr?ng thái gi??ng
            bed.Status = BedStatus.Occupied;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"?ã nh?p vi?n thành công! Mã: {admissionCode}";
            return RedirectToAction(nameof(Detail), new { id = admission.AdmissionId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"L?i: {ex.Message}";
            return RedirectToAction(nameof(Admit), new { patientId });
        }
    }

    // Xuat vien - GET
    public async Task<IActionResult> Discharge(int id)
    {
        var admission = await _db.Admissions
            .Include(a => a.Patient)
            .Include(a => a.Bed).ThenInclude(b => b!.Ward)
            .Include(a => a.AttendingDoctor)
            .FirstOrDefaultAsync(a => a.AdmissionId == id);

        if (admission == null || admission.Status != AdmissionStatus.Active)
        {
            return NotFound();
        }

        return View(admission);
    }

    // Xuat vien - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Discharge(int id, string dischargeSummary, string? dischargeInstructions)
    {
        var admission = await _db.Admissions
            .Include(a => a.Bed)
            .FirstOrDefaultAsync(a => a.AdmissionId == id);

        if (admission == null || admission.Status != AdmissionStatus.Active)
        {
            TempData["Error"] = "Khong tim thay ho so hoac da xuat vien!";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Lay Staff ID
            var doctorEmail = User.Identity!.Name;
            var staff = await _db.Staffs.FirstOrDefaultAsync(s => s.FullName == doctorEmail);

            admission.Status = AdmissionStatus.Discharged;
            admission.DischargedAt = DateTime.UtcNow;
            admission.DischargeSummary = dischargeSummary;
            admission.DischargeInstructions = dischargeInstructions;
            admission.DischargedBy = staff?.StaffId;

            // Cap nhat trang thai giuong
            if (admission.Bed != null)
            {
                admission.Bed.Status = BedStatus.Cleaning;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Da xuat vien thanh cong!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Loi: {ex.Message}";
            return RedirectToAction(nameof(Discharge), new { id });
        }
    }

    // Ghi nhan sinh hieu - GET
    public async Task<IActionResult> AddVitalSign(int admissionId)
    {
        var admission = await _db.Admissions
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.AdmissionId == admissionId);

        if (admission == null || admission.Status != AdmissionStatus.Active)
        {
            return NotFound();
        }

        ViewBag.Admission = admission;
        return View();
    }

    // Ghi nhan sinh hieu - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVitalSign(int admissionId, decimal? temperature, int? heartRate, 
        int? respiratoryRate, int? bpSystolic, int? bpDiastolic, decimal? oxygenSat, decimal? weight, decimal? height, string? note)
    {
        try
        {
            var doctorEmail = User.Identity!.Name;
            var staff = await _db.Staffs.FirstOrDefaultAsync(s => s.FullName == doctorEmail);

            var vitalSign = new VitalSign
            {
                AdmissionId = admissionId,
                RecordedAt = DateTime.UtcNow,
                RecordedBy = staff?.StaffId ?? 0,
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

            TempData["Success"] = "Da ghi nhan sinh hieu!";
            return RedirectToAction(nameof(Detail), new { id = admissionId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Loi: {ex.Message}";
            return RedirectToAction(nameof(AddVitalSign), new { admissionId });
        }
    }
}
