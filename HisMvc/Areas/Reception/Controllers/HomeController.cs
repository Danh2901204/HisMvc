using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
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
    public HomeController(AppDbContext db) => _db = db;

    // Trang lịch hẹn theo ngày với filter
    public async Task<IActionResult> Index(DateOnly? date, string status = "")
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.Today);

        var query = _db.Appointments
            .Include(x => x.Patient)
            .Include(x => x.Department)
            .Include(x => x.Doctor)
            .Include(x => x.TimeSlot)
            .Where(x => x.Date == d);

        // Filter theo status nếu có
        if (!string.IsNullOrEmpty(status))
        {
            if (status == "Booked")
                query = query.Where(x => x.Status == AppointmentStatus.Booked);
            else if (status == "CheckedIn")
            {
                // Lấy các appointment đã check-in (có Encounter)
                var checkedInIds = await _db.Encounters
                    .Where(e => e.Appointment!.Date == d)
                    .Select(e => e.AppointmentId)
                    .ToListAsync();
                query = query.Where(x => checkedInIds.Contains(x.AppointmentId));
            }
            else if (status == "Cancelled")
                query = query.Where(x => x.Status == AppointmentStatus.Cancelled);
        }

        var appts = await query
            .OrderBy(x => x.TimeSlot!.Start)
            .ToListAsync();

        // Lấy danh sách appointment đã check-in
        var checkedInAppointments = await _db.Encounters
            .Where(e => e.Appointment!.Date == d && e.AppointmentId.HasValue)
            .Select(e => e.AppointmentId.Value)
            .ToListAsync();

        ViewBag.Date = d;
        ViewBag.CheckedInAppointments = checkedInAppointments;
        ViewBag.CurrentStatus = status;
        return View(appts);
    }

    // Form tạo lịch hẹn
    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = new SelectList(await _db.Departments.ToListAsync(), "DepartmentId", "Name");
        ViewBag.Doctors = new SelectList(await _db.Staffs.Where(x => x.StaffType == "DOCTOR").ToListAsync(), "StaffId", "FullName");

        // Hiển thị giờ cụ thể thay vì Code
        var slots = await _db.TimeSlots.OrderBy(x => x.Start).ToListAsync();
        ViewBag.Slots = new SelectList(
            slots.Select(s => new
            {
                s.TimeSlotId,
                DisplayText = $"{s.Start:HH:mm} - {s.End:HH:mm}"
            }),
            "TimeSlotId",
            "DisplayText"
        );

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string fullName,
        string phone,
        DateOnly? dob,
        Gender? gender,
        int departmentId,
        int doctorId,
        int slotId,
        DateOnly date,
        string? note
    )
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
        {
            TempData["Error"] = "Vui long nhap day du ho ten va so dien thoai!";
            return RedirectToAction(nameof(Create));
        }

        // Tạo hoặc cập nhật bệnh nhân
        var patient = await _db.Patients.FirstOrDefaultAsync(x => x.Phone == phone);
        if (patient == null)
        {
            patient = new Patient
            {
                FullName = fullName,
                Phone = phone,
                Dob = dob,
                Gender = gender ?? Gender.Unknown
            };
            _db.Patients.Add(patient);
        }
        else
        {
            // Cập nhật thông tin nếu đã tồn tại
            patient.FullName = fullName;
            if (dob.HasValue) patient.Dob = dob;
            if (gender.HasValue) patient.Gender = gender.Value;
        }

        await _db.SaveChangesAsync();

        // Tạo mã lịch hẹn
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

        TempData["Success"] = $"Da tao lich hen thanh cong! Ma: {code}";
        return RedirectToAction(nameof(Index), new { date = date.ToString("yyyy-MM-dd") });
    }

    // Check-in → tạo Encounter
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int appointmentId)
    {
        var appt = await _db.Appointments
            .Include(x => x.Patient)
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

        if (appt == null)
        {
            TempData["Error"] = "Khong tim thay lich hen!";
            return RedirectToAction(nameof(Index));
        }

        if (appt.Status != AppointmentStatus.Booked)
        {
            TempData["Error"] = "Lich hen khong o trang thai Booked!";
            return RedirectToAction(nameof(Index), new { date = appt.Date.ToString("yyyy-MM-dd") });
        }

        // Kiểm tra đã check-in chưa
        var exists = await _db.Encounters
            .AnyAsync(x => x.AppointmentId == appointmentId);
        if (exists)
        {
            TempData["Error"] = "Lich hen da duoc check-in!";
            return RedirectToAction(nameof(Index), new { date = appt.Date.ToString("yyyy-MM-dd") });
        }

        // Tạo Encounter
        var enc = new Encounter
        {
            PatientId = appt.PatientId,
            AppointmentId = appt.AppointmentId,
            DoctorId = appt.DoctorId!.Value,
            Status = EncounterStatus.CheckedIn,
            CheckInAt = DateTime.UtcNow
        };

        _db.Encounters.Add(enc);

        // Cập nhật trạng thái appointment
        appt.Status = AppointmentStatus.Booked; // Giữ nguyên, hoặc có thể thêm enum mới

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Da check-in thanh cong cho benh nhan: {appt.Patient?.FullName}";
        return RedirectToAction(nameof(Index), new { date = appt.Date.ToString("yyyy-MM-dd") });
    }

    // Hủy lịch hẹn
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int appointmentId)
    {
        var appt = await _db.Appointments.FindAsync(appointmentId);
        if (appt == null)
        {
            TempData["Error"] = "Khong tim thay lich hen!";
            return RedirectToAction(nameof(Index));
        }

        appt.Status = AppointmentStatus.Cancelled;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Da huy lich hen thanh cong!";
        return RedirectToAction(nameof(Index), new { date = appt.Date.ToString("yyyy-MM-dd") });
    }

    // Tìm kiếm bệnh nhân
    [HttpGet]
    public async Task<IActionResult> SearchPatient(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Json(new { found = false });

        var patient = await _db.Patients
            .FirstOrDefaultAsync(x => x.Phone == phone);

        if (patient == null)
            return Json(new { found = false });

        return Json(new
        {
            found = true,
            patientId = patient.PatientId,
            fullName = patient.FullName,
            phone = patient.Phone,
            dob = patient.Dob?.ToString("yyyy-MM-dd"),
            gender = patient.Gender.ToString()
        });
    }

    // Trang quản lý bệnh nhân
    public async Task<IActionResult> Patients(string search = "")
    {
        var query = _db.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.FullName.Contains(search) || x.Phone.Contains(search));
        }

        var patients = await query
            .OrderByDescending(x => x.PatientId)
            .Take(100)
            .ToListAsync();

        ViewBag.Search = search;
        return View(patients);
    }

    // Tạo bệnh nhân mới
    public IActionResult CreatePatient()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePatient(Patient model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Kiểm tra trùng số điện thoại
        if (await _db.Patients.AnyAsync(x => x.Phone == model.Phone))
        {
            ModelState.AddModelError("Phone", "So dien thoai da ton tai!");
            return View(model);
        }

        _db.Patients.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Da them benh nhan thanh cong!";
        return RedirectToAction(nameof(Patients));
    }

    // Sửa thông tin bệnh nhân
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
        {
            return View(model);
        }

        _db.Patients.Update(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Da cap nhat thong tin benh nhan!";
        return RedirectToAction(nameof(Patients));
    }
}
