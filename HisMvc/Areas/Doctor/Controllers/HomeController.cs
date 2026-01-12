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
        decimal examFee = 100000;
        decimal totalOrderPrice = orders.Sum(x => x.Service?.Price ?? 0);
        decimal totalAmount = examFee + totalOrderPrice;

        // Tạo mã hóa đơn
        var invoiceCode = $"INV{DateTime.Now:yyyyMMddHHmmss}";

        // Tạo hóa đơn
        var invoice = new Invoice
        {
            EncounterId = encounterId,
            InvoiceCode = invoiceCode,
            TotalAmount = totalAmount,
            Status = InvoiceStatus.Unpaid,
            CreatedAt = DateTime.UtcNow,
            Note = "Tu dong tao khi chot luot kham"
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();
    }

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

