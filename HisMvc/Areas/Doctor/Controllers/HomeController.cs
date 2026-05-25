using HisMvc.Areas.Doctor.Models;
using HisMvc.Areas.Doctor.Services;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Services;
using HisMvc.Services.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = AppRoles.DOCTOR + "," + AppRoles.ADMIN)]
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly DoctorViewService _views;
    private readonly CurrentStaffService _staffService;
    private readonly OutpatientWorkflowService _workflow;
    private readonly Icd10Service _icd10;

    public HomeController(AppDbContext db, DoctorViewService views, CurrentStaffService staffService,
        OutpatientWorkflowService workflow, Icd10Service icd10)
    {
        _db = db;
        _views = views;
        _staffService = staffService;
        _workflow = workflow;
        _icd10 = icd10;
    }

    public async Task<IActionResult> Dashboard()
    {
        var model = await _views.BuildDashboardAsync();
        return View(model);
    }

    public async Task<IActionResult> Index(string status = "")
    {
        var model = await _views.GetEncounterListAsync(status);
        return View(model);
    }

    public async Task<IActionResult> Examine(int id)
    {
        var model = await _views.GetExamineAsync(id);
        if (model == null)
            return NotFound();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> SearchIcd10(string q)
    {
        var items = await _icd10.SearchAsync(q, 20);
        return Json(items.Select(x => new { code = x.Code, name = x.Name, chapter = x.Chapter }));
    }

    [HttpGet]
    public async Task<IActionResult> ParseIcd10(string diagnosis)
    {
        var parsed = await _icd10.ParseDiagnosisAsync(diagnosis);
        return Json(new
        {
            primaryCode = parsed.PrimaryCode,
            primaryName = parsed.PrimaryName,
            secondaryCodes = parsed.SecondaryCodes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CallPatient(int id, string? roomNumber)
    {
        var result = await _workflow.CallPatientAsync(id, roomNumber);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(result.Success ? nameof(Examine) : nameof(Dashboard), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string? diagnosis, string? conclusion,
        string? icd10Primary, string? icd10PrimaryName, string? icd10Secondary,
        string? instructions, DateOnly? followUpDate)
    {
        var enc = await _db.Encounters.FirstOrDefaultAsync(x => x.EncounterId == id);
        if (enc == null)
        {
            TempData["Error"] = "Không tìm thay lượt khám!";
            return RedirectToAction(nameof(Index));
        }

        if (enc.Status == EncounterStatus.Completed || enc.Status == EncounterStatus.Cancelled)
        {
            TempData["Error"] = "Lượt khám đã chốt/huy, không thể sua!";
            return RedirectToAction(nameof(Examine), new { id });
        }

        if (enc.Status == EncounterStatus.WaitingExam || enc.Status == EncounterStatus.CheckedIn)
        {
            TempData["Error"] = "BN chưa được gọi vào phòng khám. Vui lòng bấm 'Gọi BN' trước.";
            return RedirectToAction(nameof(Examine), new { id });
        }

        enc.Diagnosis = (diagnosis ?? "").Trim();
        enc.Conclusion = (conclusion ?? "").Trim();
        enc.Icd10Primary = string.IsNullOrWhiteSpace(icd10Primary) ? null : icd10Primary.Trim().ToUpperInvariant();
        enc.Icd10PrimaryName = string.IsNullOrWhiteSpace(icd10PrimaryName) ? null : icd10PrimaryName.Trim();
        enc.Icd10Secondary = string.IsNullOrWhiteSpace(icd10Secondary) ? null : icd10Secondary.Trim().ToUpperInvariant();
        enc.Instructions = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim();
        enc.FollowUpDate = followUpDate;

        // Tu dong tach ma ICD-10 từ ô chẩn đoán nếu BS chưa chọn ma
        await _icd10.ApplyParsedToEncounterAsync(enc);

        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã lưu thông tin khám!";
        return RedirectToAction(nameof(Examine), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOrder(int encounterId, int serviceId, int quantity = 1)
    {
        var enc = await _db.Encounters.FirstOrDefaultAsync(x => x.EncounterId == encounterId);
        if (enc == null) { TempData["Error"] = "Không tìm thay lượt khám!"; return RedirectToAction(nameof(Index)); }

        if (enc.Status != EncounterStatus.InService && enc.Status != EncounterStatus.WaitingResult)
        {
            TempData["Error"] = "Không thể chi dinh CLS o trạng thái nay.";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        var service = await _db.Services.FindAsync(serviceId);
        if (service == null) { TempData["Error"] = "Không tìm thay dịch vụ!"; return RedirectToAction(nameof(Examine), new { id = encounterId }); }

        var staffId = await _staffService.TryGetStaffIdAsync(User);

        var order = new Order
        {
            OrderCode = $"OR{DateTime.Now:yyyyMMddHHmmss}",
            EncounterId = encounterId,
            ServiceId = serviceId,
            Quantity = Math.Max(1, quantity),
            Status = OrderStatus.Requested,
            OrderedBy = User.Identity?.Name ?? "doctor",
            OrderedByStaffId = staffId,
            OrderedAt = DateTime.UtcNow
        };
        _db.Orders.Add(order);

        if (enc.Status == EncounterStatus.InService)
            enc.Status = EncounterStatus.WaitingResult;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã chi dinh: {service.Name} (x{order.Quantity})";
        return RedirectToAction(nameof(Examine), new { id = encounterId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int orderId, int encounterId)
    {
        var order = await _db.Orders
            .Include(x => x.OrderResult)
            .FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (order == null)
        {
            TempData["Error"] = "Không tìm thay chi dinh!";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        if (order.Status != OrderStatus.Requested && order.Status != OrderStatus.InProgress)
        {
            TempData["Error"] = "Chỉ định da có kết quả, không thể huy!";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        order.Status = OrderStatus.Cancelled;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã huy chi dinh!";
        return RedirectToAction(nameof(Examine), new { id = encounterId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        var result = await _workflow.CloseEncounterAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(result.Success ? nameof(Index) : nameof(Examine), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePrescription(int encounterId)
    {
        var enc = await _db.Encounters.FirstOrDefaultAsync(x => x.EncounterId == encounterId);
        if (enc == null)
        {
            TempData["Error"] = "Không tìm thay lượt khám!";
            return RedirectToAction(nameof(Index));
        }

        if (enc.Status != EncounterStatus.InService && enc.Status != EncounterStatus.WaitingResult)
        {
            TempData["Error"] = "Chi co the ke don khi Đang khám (InService/WaitingResult)!";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        var existing = await _db.Prescriptions.FirstOrDefaultAsync(p => p.EncounterId == encounterId);
        if (existing != null)
        {
            TempData["Error"] = "Đã co đơn thuốc cho lượt khám nay!";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        var prescribedBy = await _staffService.GetStaffIdAsync(User, enc.DoctorId);

        var prescription = new Prescription
        {
            Code = $"PRE{DateTime.UtcNow:yyyyMMddHHmmss}",
            EncounterId = encounterId,
            PrescribedBy = prescribedBy,
            PrescribedAt = DateTime.UtcNow,
            Status = PrescriptionStatus.Pending
        };

        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã tao đơn thuốc!";
        return RedirectToAction(nameof(Examine), new { id = encounterId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMedicine(int prescriptionId, int medicineId, int quantity, string dosage, string? instructions, int duration = 1)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);

        if (prescription == null)
        {
            TempData["Error"] = "Không tìm thay đơn thuốc!";
            return RedirectToAction(nameof(Index));
        }

        if (prescription.Status != PrescriptionStatus.Pending)
        {
            TempData["Error"] = "Don thuốc đã cấp phát, không thể sua!";
            return RedirectToAction(nameof(Examine), new { id = prescription.EncounterId });
        }

        if (prescription.Items.Any(i => i.MedicineId == medicineId))
        {
            TempData["Error"] = "Thuốc đã có trong don!";
            return RedirectToAction(nameof(Examine), new { id = prescription.EncounterId });
        }

        var item = new PrescriptionItem
        {
            PrescriptionId = prescriptionId,
            MedicineId = medicineId,
            Quantity = quantity,
            Dosage = dosage,
            Instructions = instructions,
            Duration = duration
        };

        _db.PrescriptionItems.Add(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã them thuốc vào đơn!";
        return RedirectToAction(nameof(Examine), new { id = prescription.EncounterId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMedicine(int itemId, int encounterId)
    {
        var item = await _db.PrescriptionItems
            .Include(i => i.Prescription)
            .FirstOrDefaultAsync(i => i.PrescriptionItemId == itemId);

        if (item == null || item.Prescription!.Status != PrescriptionStatus.Pending)
        {
            TempData["Error"] = "Không thể xóa thuốc!";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        _db.PrescriptionItems.Remove(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã xóa thuốc khỏi đơn!";
        return RedirectToAction(nameof(Examine), new { id = encounterId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reopen(int id)
    {
        var enc = await _db.Encounters.FirstOrDefaultAsync(x => x.EncounterId == id);
        if (enc == null) { TempData["Error"] = "Không tìm thay lượt khám!"; return RedirectToAction(nameof(Index)); }

        if (enc.Status != EncounterStatus.WaitingFinalPayment)
        {
            TempData["Error"] = "Chi co the mở lại khi Đang chờ thu chi phí phát sinh.";
            return RedirectToAction(nameof(Examine), new { id });
        }

        var paidFinal = await _db.Invoices.AnyAsync(i =>
            i.EncounterId == id && i.InvoiceType == InvoiceType.Final && i.Status == InvoiceStatus.Paid);
        if (paidFinal)
        {
            TempData["Error"] = "Hoa don tong hop da thanh toán, không thể mở lại.";
            return RedirectToAction(nameof(Examine), new { id });
        }

        enc.Status = EncounterStatus.InService;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã mở lại lượt khám!";
        return RedirectToAction(nameof(Examine), new { id });
    }
}
