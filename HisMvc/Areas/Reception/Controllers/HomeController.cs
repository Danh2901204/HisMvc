using HisMvc.Areas.Reception.Models;
using HisMvc.Areas.Reception.Services;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Services;
using HisMvc.Services.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Reception.Controllers;

[Area("Reception")]
[Authorize(Roles = AppRoles.RECEPTION + "," + AppRoles.ADMIN)]
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly ReceptionViewService _views;
    private readonly OutpatientWorkflowService _workflow;
    private readonly IAppointmentCancellationService _cancellation;

    public HomeController(
        AppDbContext db,
        ReceptionViewService views,
        OutpatientWorkflowService workflow,
        IAppointmentCancellationService cancellation)
    {
        _db = db;
        _views = views;
        _workflow = workflow;
        _cancellation = cancellation;
    }

    public async Task<IActionResult> Dashboard()
    {
        var model = await _views.BuildDashboardAsync();
        return View(model);
    }

    public async Task<IActionResult> Index(DateOnly? date, string status = "")
    {
        var model = await _views.GetAppointmentListAsync(date, status);
        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = new SelectList(await _db.Departments.ToListAsync(), "DepartmentId", "Name");
        ViewBag.Doctors = new SelectList(await _db.Staffs.Where(x => x.StaffType == "DOCTOR").ToListAsync(), "StaffId", "FullName");

        var slots = await _db.TimeSlots.OrderBy(x => x.Start).ToListAsync();
        ViewBag.Slots = new SelectList(
            slots.Select(s => new { s.TimeSlotId, DisplayText = $"{s.Start:HH:mm} - {s.End:HH:mm}" }),
            "TimeSlotId",
            "DisplayText"
        );

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string fullName, string phone, DateOnly? dob, Gender? gender,
        string? identityNumber, string? address, string? insuranceNumber,
        string? insuranceType, DateTime? insuranceExpiry, decimal insuranceCoveragePercent,
        string? insuranceHospital, int departmentId, int doctorId, int slotId,
        DateOnly date, string? note)
    {
        phone = (phone ?? "").Trim();
        fullName = (fullName ?? "").Trim();

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
        {
            TempData["Error"] = "Vui lòng nhập đầy đủ ho ten va so dien thoai!";
            return RedirectToAction(nameof(Create));
        }

        var patient = await _db.Patients.FirstOrDefaultAsync(x => x.Phone == phone);
        if (patient == null)
        {
            patient = new Patient
            {
                FullName = fullName,
                Phone = phone,
                Dob = dob,
                Gender = gender ?? Gender.Unknown,
                InsuranceCoveragePercent = insuranceCoveragePercent
            };
            _db.Patients.Add(patient);
        }

        ApplyPatientDetails(patient, fullName, dob, gender, identityNumber, address,
            insuranceNumber, insuranceType, insuranceExpiry, insuranceCoveragePercent, insuranceHospital);

        await _db.SaveChangesAsync();

        var code = $"APT{DateTime.Now:yyyyMMddHHmmss}";

        var appt = new Appointment
        {
            Code = code,
            PatientId = patient.PatientId,
            DepartmentId = departmentId,
            DoctorId = doctorId,
            Date = date,
            TimeSlotId = slotId,
            Status = AppointmentStatus.Booked,
            Note = note
        };

        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã tạo lịch hẹn thành công! Mã: {code}";
        return RedirectToAction(nameof(Index), new { date = date.ToString("yyyy-MM-dd") });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int appointmentId)
    {
        var result = await _workflow.CheckInAsync(appointmentId);

        TempData[result.Success ? "Success" : "Error"] = result.Message;

        var appt = await _db.Appointments.FindAsync(appointmentId);
        var date = appt?.Date.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
        return RedirectToAction(nameof(Index), new { date });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int appointmentId)
    {
        var result = await _cancellation.CancelByReceptionAsync(appointmentId);

        TempData[result.Success ? "Success" : "Error"] = result.Message;

        var appt = await _db.Appointments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        var date = appt?.Date.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
        return RedirectToAction(nameof(Index), new { date });
    }

    [HttpGet]
    public async Task<IActionResult> SearchPatient(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Json(new { found = false });

        var patient = await _db.Patients.FirstOrDefaultAsync(x => x.Phone == phone);

        if (patient == null)
            return Json(new { found = false });

        return Json(new
        {
            found = true,
            patientId = patient.PatientId,
            fullName = patient.FullName,
            phone = patient.Phone,
            dob = patient.Dob?.ToString("yyyy-MM-dd"),
            gender = patient.Gender.ToString(),
            identityNumber = patient.IdentityNumber,
            address = patient.Address,
            insuranceNumber = patient.InsuranceNumber,
            insuranceType = patient.InsuranceType,
            insuranceExpiry = patient.InsuranceExpiry?.ToString("yyyy-MM-dd"),
            insuranceCoveragePercent = patient.InsuranceCoveragePercent,
            insuranceHospital = patient.InsuranceHospital
        });
    }

    private static void ApplyPatientDetails(
        Patient patient, string fullName, DateOnly? dob, Gender? gender,
        string? identityNumber, string? address, string? insuranceNumber,
        string? insuranceType, DateTime? insuranceExpiry, decimal insuranceCoveragePercent,
        string? insuranceHospital)
    {
        patient.FullName = fullName;
        patient.Dob = dob;
        patient.Gender = gender ?? patient.Gender;
        patient.IdentityNumber = string.IsNullOrWhiteSpace(identityNumber) ? null : identityNumber.Trim();
        patient.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        patient.InsuranceNumber = string.IsNullOrWhiteSpace(insuranceNumber) ? null : insuranceNumber.Trim();
        patient.InsuranceType = string.IsNullOrWhiteSpace(insuranceType) ? null : insuranceType.Trim();
        patient.InsuranceExpiry = insuranceExpiry;
        patient.InsuranceCoveragePercent = insuranceCoveragePercent;
        patient.InsuranceHospital = string.IsNullOrWhiteSpace(insuranceHospital) ? null : insuranceHospital.Trim();
    }

    public async Task<IActionResult> Patients(string search = "")
    {
        var query = _db.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.FullName.Contains(search) || x.Phone.Contains(search));

        var patients = await query.OrderByDescending(x => x.PatientId).Take(100).ToListAsync();

        ViewBag.Search = search;
        return View(patients);
    }

    public IActionResult CreatePatient() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePatient(Patient model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _db.Patients.AnyAsync(x => x.Phone == model.Phone))
        {
            ModelState.AddModelError("Phone", "So dien thoai da tồn tại!");
            return View(model);
        }

        _db.Patients.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã them bệnh nhân thành công!";
        return RedirectToAction(nameof(Patients));
    }

    public async Task<IActionResult> EditPatient(int id)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient == null)
            return NotFound();
        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPatient(Patient model)
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

        ApplyPatientDetails(patient, model.FullName, model.Dob, model.Gender,
            model.IdentityNumber, model.Address, model.InsuranceNumber, model.InsuranceType,
            model.InsuranceExpiry, model.InsuranceCoveragePercent, model.InsuranceHospital);
        patient.Phone = model.Phone;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã cập nhật thông tin bệnh nhân!";
        return RedirectToAction(nameof(Patients));
    }
}
