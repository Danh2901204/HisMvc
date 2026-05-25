using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = AppRoles.DOCTOR + "," + AppRoles.ADMIN)]
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    // Danh sách lượt khám (Encounter) với filter
    public async Task<IActionResult> Index(string status = "")
    {
        var query = _db.Encounters
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .Include(x => x.Appointment)
            .AsQueryable();

        // Filter theo status
        if (!string.IsNullOrEmpty(status))
        {
            if (status == "CheckedIn")
                query = query.Where(x => x.Status == EncounterStatus.CheckedIn);
            else if (status == "InService")
                query = query.Where(x => x.Status == EncounterStatus.InService);
            else if (status == "Completed")
                query = query.Where(x => x.Status == EncounterStatus.Completed);
        }

        var items = await query
            .OrderByDescending(x => x.CheckInAt)
            .ToListAsync();

        ViewBag.CurrentStatus = status;
        return View(items);
    }

    // Màn hình khám chi tiết
    public async Task<IActionResult> Examine(int id)
    {
        var enc = await _db.Encounters
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .Include(x => x.Appointment)
            .FirstOrDefaultAsync(x => x.EncounterId == id);

        if (enc == null)
            return NotFound();

        // Lấy danh sách Orders
        var orders = await _db.Orders
            .Include(o => o.Service)
            .Include(o => o.OrderResult)
            .Where(o => o.EncounterId == id)
            .OrderByDescending(o => o.OrderedAt)
            .ToListAsync();

        ViewBag.Orders = orders;
        ViewBag.Services = new SelectList(
            await _db.Services.OrderBy(s => s.Type).ThenBy(s => s.Name).ToListAsync(),
            "ServiceId",
            "Name"
        );

        // Lấy đơn thuốc nếu có
        var prescription = await _db.Prescriptions
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.EncounterId == id);

        ViewBag.Prescription = prescription;
        
        // Danh sách thuốc để kê đơn
        ViewBag.Medicines = new SelectList(
            await _db.Medicines.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync(),
            "MedicineId",
            "Name"
        );

        return View(enc);
    }

    // Lưu chẩn đoán/kết luận
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string? diagnosis, string? conclusion)
    {
        var enc = await _db.Encounters.FirstOrDefaultAsync(x => x.EncounterId == id);
        if (enc == null)
        {
            TempData["Error"] = "Khong tim thay luot kham!";
            return RedirectToAction(nameof(Index));
        }

        if (enc.Status == EncounterStatus.Completed)
        {
            TempData["Error"] = "Luot kham da chot, khong the sua!";
            return RedirectToAction(nameof(Examine), new { id });
        }

        enc.Diagnosis = (diagnosis ?? "").Trim();
        enc.Conclusion = (conclusion ?? "").Trim();
        
        // Chuyển sang trạng thái InService khi bắt đầu nhập thông tin
        if (enc.Status == EncounterStatus.CheckedIn)
            enc.Status = EncounterStatus.InService;

        await _db.SaveChangesAsync();
        
        TempData["Success"] = "Da luu thong tin kham!";
        return RedirectToAction(nameof(Examine), new { id });
    }

    // Chỉ định dịch vụ (tạo Order)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOrder(int encounterId, int serviceId)
    {
        var enc = await _db.Encounters.FirstOrDefaultAsync(x => x.EncounterId == encounterId);
        if (enc == null)
        {
            TempData["Error"] = "Khong tim thay luot kham!";
            return RedirectToAction(nameof(Index));
        }

        if (enc.Status == EncounterStatus.Completed)
        {
            TempData["Error"] = "Luot kham da chot, khong the chi dinh!";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        var service = await _db.Services.FindAsync(serviceId);
        if (service == null)
        {
            TempData["Error"] = "Khong tim thay dich vu!";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        var order = new Order
        {
            EncounterId = encounterId,
            ServiceId = serviceId,
            Status = OrderStatus.Requested,
            OrderedBy = User.Identity?.Name ?? "doctor",
            OrderedAt = DateTime.UtcNow
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Da chi dinh: {service.Name}";
        return RedirectToAction(nameof(Examine), new { id = encounterId });
    }

    // Hủy chỉ định (chỉ khi chưa có kết quả)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int orderId, int encounterId)
    {
        var order = await _db.Orders
            .Include(x => x.OrderResult)
            .FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (order == null)
        {
            TempData["Error"] = "Khong tim thay chi dinh!";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        if (order.Status != OrderStatus.Requested)
        {
            TempData["Error"] = "Chi dinh da co ket quả, khong the huy!";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        order.Status = OrderStatus.Cancelled;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Da huy chi dinh!";
        return RedirectToAction(nameof(Examine), new { id = encounterId });
    }

    // Chốt lượt khám
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        var enc = await _db.Encounters.FirstOrDefaultAsync(x => x.EncounterId == id);
        if (enc == null)
        {
            TempData["Error"] = "Khong tim thay luot kham!";
            return RedirectToAction(nameof(Index));
        }

        if (enc.Status == EncounterStatus.Completed)
        {
            TempData["Error"] = "Luot kham da duoc chot!";
            return RedirectToAction(nameof(Examine), new { id });
        }

        // Kiểm tra chẩn đoán
        if (string.IsNullOrWhiteSpace(enc.Diagnosis))
        {
            TempData["Error"] = "Chua co chan doan! Vui long nhap chan doan truoc khi chot.";
            return RedirectToAction(nameof(Examine), new { id });
        }

        // Kiểm tra còn order Requested không
        var pendingOrders = await _db.Orders
            .Where(o => o.EncounterId == id && o.Status == OrderStatus.Requested)
            .ToListAsync();

        if (pendingOrders.Any())
        {
            TempData["Error"] = $"Con {pendingOrders.Count} chi dinh chua co ket qua! Khong the chot luot kham.";
            return RedirectToAction(nameof(Examine), new { id });
        }

        // Chốt lượt khám
        enc.Status = EncounterStatus.Completed;
        enc.EndAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Tự động tạo hóa đơn
        await CreateInvoiceForEncounter(id);

        TempData["Success"] = "Da chot luot kham va tao hoa don thanh cong! Benh nhan co the thanh toan.";
        return RedirectToAction(nameof(Index));
    }

    // Helper method để tạo hóa đơn
    private async Task CreateInvoiceForEncounter(int encounterId)
    {
        // Kiểm tra đã có hóa đơn chưa
        var existingInvoice = await _db.Invoices
            .FirstOrDefaultAsync(x => x.EncounterId == encounterId);

        if (existingInvoice != null)
            return; // Đã có hóa đơn rồi

        // Lấy danh sách dịch vụ
        var orders = await _db.Orders
            .Include(x => x.Service)
            .Where(x => x.EncounterId == encounterId)
            .ToListAsync();

        // Tính tổng tiền
        decimal examFee = HisConstants.EXAM_FEE;
        decimal totalOrderPrice = orders.Sum(x => x.Service?.Price ?? 0);
        decimal totalAmount = examFee + totalOrderPrice;

        // Tạo mã hóa đơn
        var invoiceCode = $"INV{DateTime.Now:yyyyMMddHHmmss}";

        // Tạo hóa đơn (không tự động tính BHYT ở đây, để thu ngân xử lý)
        var invoice = new Invoice
        {
            EncounterId = encounterId,
            InvoiceCode = invoiceCode,
            TotalAmount = totalAmount,
            PatientAmount = totalAmount, // Mặc định bệnh nhân trả toàn bộ
            HasInsurance = false,
            Status = InvoiceStatus.Unpaid,
            CreatedAt = DateTime.UtcNow,
            Note = "Tu dong tao khi chot luot kham"
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();
    }

    // ========== KÊ ĐƠN THUỐC ==========

    // Kê đơn thuốc mới
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePrescription(int encounterId)
    {
        var enc = await _db.Encounters.FirstOrDefaultAsync(x => x.EncounterId == encounterId);
        if (enc == null)
        {
            TempData["Error"] = "Không tìm thấy lượt khám!";
            return RedirectToAction(nameof(Index));
        }

        if (enc.Status == EncounterStatus.Completed)
        {
            TempData["Error"] = "Lượt khám đã chốt, không thể kê đơn!";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        // Kiểm tra đã có đơn thuốc chưa
        var existing = await _db.Prescriptions.FirstOrDefaultAsync(p => p.EncounterId == encounterId);
        if (existing != null)
        {
            TempData["Error"] = "Đã có đơn thuốc cho lượt khám này!";
            return RedirectToAction(nameof(Examine), new { id = encounterId });
        }

        // Lấy StaffId của bác sĩ
        var doctorEmail = User.Identity!.Name;
        var staff = await _db.Staffs.FirstOrDefaultAsync(s => s.FullName == doctorEmail);

        var prescription = new Prescription
        {
            Code = $"PRE{DateTime.UtcNow:yyyyMMddHHmmss}",
            EncounterId = encounterId,
            PrescribedBy = staff?.StaffId ?? enc.DoctorId,
            PrescribedAt = DateTime.UtcNow,
            Status = PrescriptionStatus.Pending
        };

        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã tạo đơn thuốc!";
        return RedirectToAction(nameof(Examine), new { id = encounterId });
    }

    // Thêm thuốc vào đơn
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMedicine(int prescriptionId, int medicineId, int quantity, string dosage, string? instructions, int duration = 1)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);

        if (prescription == null)
        {
            TempData["Error"] = "Không tìm thấy đơn thuốc!";
            return RedirectToAction(nameof(Index));
        }

        if (prescription.Status != PrescriptionStatus.Pending)
        {
            TempData["Error"] = "Đơn thuốc đã cấp phát, không thể sửa!";
            return RedirectToAction(nameof(Examine), new { id = prescription.EncounterId });
        }

        // Kiểm tra thuốc đã có trong đơn chưa
        if (prescription.Items.Any(i => i.MedicineId == medicineId))
        {
            TempData["Error"] = "Thuốc đã có trong đơn!";
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

        TempData["Success"] = "Đã thêm thuốc vào đơn!";
        return RedirectToAction(nameof(Examine), new { id = prescription.EncounterId });
    }

    // Xóa thuốc khỏi đơn
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

    // ========== END KÊ ĐƠN THUỐC ==========

    // Mở lại lượt khám (nếu cần)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reopen(int id)
    {
        var enc = await _db.Encounters.FirstOrDefaultAsync(x => x.EncounterId == id);
        if (enc == null)
        {
            TempData["Error"] = "Khong tim thay luot kham!";
            return RedirectToAction(nameof(Index));
        }

        if (enc.Status != EncounterStatus.Completed)
        {
            TempData["Error"] = "Luot kham chua duoc chot!";
            return RedirectToAction(nameof(Examine), new { id });
        }

        enc.Status = EncounterStatus.InService;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Da mo lai luot kham!";
        return RedirectToAction(nameof(Examine), new { id });
    }
}

