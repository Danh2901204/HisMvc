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
public class MedicalOrderController : Controller
{
    private readonly AppDbContext _db;
    private readonly InpatientViewService _views;
    private readonly CurrentStaffService _staffService;

    public MedicalOrderController(AppDbContext db, InpatientViewService views, CurrentStaffService staffService)
    {
        _db = db;
        _views = views;
        _staffService = staffService;
    }

    public async Task<IActionResult> Index(int admissionId)
    {
        var model = await _views.GetMedicalOrderListAsync(admissionId);
        if (model == null)
            return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int admissionId,
        MedicalOrderType orderType,
        string orderDetails,
        DateTime scheduledAt,
        string? note,
        int? medicineId,
        int? quantity,
        string? dosage,
        int? duration,
        int? serviceId)
    {
        var admission = await _db.Admissions.FindAsync(admissionId);
        if (admission == null || admission.Status != AdmissionStatus.Active)
        {
            TempData["Error"] = "Không tìm thay hồ sơ hoặc đã xuất viện!";
            return RedirectToAction(nameof(Index), new { admissionId });
        }

        if (string.IsNullOrWhiteSpace(orderDetails))
        {
            TempData["Error"] = "Vui lòng nhập nội dung y lenh!";
            return RedirectToAction(nameof(Index), new { admissionId });
        }

        var orderedBy = await _staffService.GetStaffIdAsync(User, admission.AttendingDoctorId);
        var orderCode = $"YL{DateTime.UtcNow:yyyyMMddHHmmss}";

        var medicalOrder = new MedicalOrder
        {
            OrderCode = orderCode,
            AdmissionId = admissionId,
            OrderType = orderType,
            OrderDetails = orderDetails.Trim(),
            OrderedAt = DateTime.UtcNow,
            ScheduledAt = scheduledAt == default ? DateTime.UtcNow : scheduledAt,
            OrderedBy = orderedBy,
            Status = MedicalOrderStatus.Ordered,
            Note = note?.Trim()
        };

        if (orderType == MedicalOrderType.Medication && medicineId.HasValue && quantity.HasValue && !string.IsNullOrWhiteSpace(dosage))
        {
            var prescription = new Prescription
            {
                Code = $"PRE{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                AdmissionId = admissionId,
                EncounterId = null,
                PrescribedBy = orderedBy,
                PrescribedAt = DateTime.UtcNow,
                Status = PrescriptionStatus.Pending,
                Note = $"Y lênh noi tru {orderCode}",
                Items = new List<PrescriptionItem>
                {
                    new PrescriptionItem
                    {
                        MedicineId = medicineId.Value,
                        Quantity = quantity.Value,
                        Dosage = dosage,
                        Duration = duration ?? 1
                    }
                }
            };
            _db.Prescriptions.Add(prescription);
            await _db.SaveChangesAsync();
            medicalOrder.PrescriptionId = prescription.PrescriptionId;
        }

        if ((orderType == MedicalOrderType.Lab || orderType == MedicalOrderType.Imaging) && serviceId.HasValue)
        {
            var encounterId = await GetOrCreateInpatientEncounterId(admission);
            _db.Orders.Add(new Order
            {
                OrderCode = $"OR{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                EncounterId = encounterId,
                ServiceId = serviceId.Value,
                Status = OrderStatus.Requested,
                OrderedBy = User.Identity?.Name ?? "doctor",
                OrderedByStaffId = orderedBy,
                OrderedAt = DateTime.UtcNow
            });
        }

        _db.MedicalOrders.Add(medicalOrder);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã ra y lenh {orderCode}!";
        return RedirectToAction(nameof(Index), new { admissionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int id)
    {
        var order = await _db.MedicalOrders.FindAsync(id);
        if (order == null || order.Status != MedicalOrderStatus.Ordered)
        {
            TempData["Error"] = "Không thể bat dau y lenh nay!";
            return RedirectToAction(nameof(Index), new { admissionId = order?.AdmissionId ?? 0 });
        }

        order.Status = MedicalOrderStatus.InProgress;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã bat dau thuc hien y lenh.";
        return RedirectToAction(nameof(Index), new { admissionId = order.AdmissionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, string? executionNote)
    {
        var order = await _db.MedicalOrders.FindAsync(id);
        if (order == null) { TempData["Error"] = "Không tìm thay y lenh!"; return RedirectToAction(nameof(Index), new { admissionId = 0 }); }
        if (order.Status != MedicalOrderStatus.InProgress)
        {
            TempData["Error"] = "Chi co the hoàn thành y lenh dang thuc hien (InProgress). Vui lòng bấm 'Bat dau' trước.";
            return RedirectToAction(nameof(Index), new { admissionId = order.AdmissionId });
        }

        var executedBy = await _staffService.TryGetStaffIdAsync(User);
        order.Status = MedicalOrderStatus.Completed;
        order.ExecutedAt = DateTime.UtcNow;
        order.ExecutedBy = executedBy;
        order.ExecutionNote = executionNote?.Trim();
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã hoàn thành y lenh.";
        return RedirectToAction(nameof(Index), new { admissionId = order.AdmissionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        var order = await _db.MedicalOrders.FindAsync(id);
        if (order == null || order.Status == MedicalOrderStatus.Completed)
        {
            TempData["Error"] = "Không thể huy y lenh nay!";
            return RedirectToAction(nameof(Index), new { admissionId = order?.AdmissionId ?? 0 });
        }

        order.Status = MedicalOrderStatus.Cancelled;
        order.ExecutionNote = $"Hủy: {reason}";
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã huy y lenh.";
        return RedirectToAction(nameof(Index), new { admissionId = order.AdmissionId });
    }

    private async Task<int> GetOrCreateInpatientEncounterId(Admission admission)
    {
        var tag = $"Noi tru: {admission.AdmissionCode}";

        var enc = await _db.Encounters
            .Where(e => e.PatientId == admission.PatientId
                     && e.Conclusion == tag
                     && e.Status != EncounterStatus.Completed
                     && e.Status != EncounterStatus.Cancelled)
            .OrderByDescending(e => e.CheckInAt)
            .FirstOrDefaultAsync();

        if (enc != null)
            return enc.EncounterId;

        var newEnc = new Encounter
        {
            EncounterCode = $"NT-{admission.AdmissionCode}",
            PatientId = admission.PatientId,
            DoctorId = admission.AttendingDoctorId,
            DepartmentId = (await _db.Wards.Where(w => w.WardId == _db.Beds.Where(b => b.BedId == admission.BedId).Select(b => b.WardId).FirstOrDefault())
                                          .Select(w => w.DepartmentId)
                                          .FirstOrDefaultAsync()),
            Status = EncounterStatus.InService,
            CheckInAt = admission.AdmittedAt,
            EndAt = admission.AdmittedAt,
            Diagnosis = admission.InitialDiagnosis,
            Conclusion = tag
        };
        _db.Encounters.Add(newEnc);
        await _db.SaveChangesAsync();
        return newEnc.EncounterId;
    }
}
