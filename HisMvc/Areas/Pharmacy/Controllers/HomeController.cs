using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Pharmacy.Controllers;

[Area("Pharmacy")]
[Authorize(Roles = AppRoles.PHARMACIST + "," + AppRoles.ADMIN)]
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    // Danh sach don thuoc cho cap phat
    public async Task<IActionResult> Index(DateTime? date, PrescriptionStatus? status)
    {
        var targetDate = date.HasValue ? DateOnly.FromDateTime(date.Value) : DateOnly.FromDateTime(DateTime.Today);

        var query = _db.Prescriptions
            .Include(p => p.Encounter).ThenInclude(e => e!.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .Where(p => DateOnly.FromDateTime(p.PrescribedAt) == targetDate);

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }
        else
        {
            // Mac dinh chi hien Pending
            query = query.Where(p => p.Status == PrescriptionStatus.Pending);
        }

        var prescriptions = await query
            .OrderBy(p => p.PrescribedAt)
            .ToListAsync();

        ViewBag.SelectedDate = targetDate;
        ViewBag.SelectedStatus = status;

        return View(prescriptions);
    }

    // Xem chi tiet don thuoc
    public async Task<IActionResult> Detail(int id)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Encounter).ThenInclude(e => e!.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.PrescriptionId == id);

        if (prescription == null)
        {
            return NotFound();
        }

        // Lay thong tin ton kho cho tung thuoc
        var medicineIds = prescription.Items.Select(i => i.MedicineId).ToList();
        var stocks = await _db.MedicineBatches
            .Where(b => medicineIds.Contains(b.MedicineId) && b.IsActive && b.ExpiryDate > DateTime.Today)
            .GroupBy(b => b.MedicineId)
            .Select(g => new
            {
                MedicineId = g.Key,
                TotalStock = g.Sum(b => b.QuantityInStock)
            })
            .ToDictionaryAsync(x => x.MedicineId, x => x.TotalStock);

        ViewBag.Stocks = stocks;

        return View(prescription);
    }

    // Cap phat thuoc - GET
    public async Task<IActionResult> Dispense(int id)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Encounter).ThenInclude(e => e!.Patient)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.PrescriptionId == id);

        if (prescription == null || prescription.Status != PrescriptionStatus.Pending)
        {
            return NotFound();
        }

        // Lay lo thuoc con han cho tung thuoc trong don
        var medicineIds = prescription.Items.Select(i => i.MedicineId).ToList();
        var batches = await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Where(b => medicineIds.Contains(b.MedicineId) 
                     && b.IsActive 
                     && b.ExpiryDate > DateTime.Today.AddMonths(1) // Còn h?n ít nh?t 1 tháng
                     && b.QuantityInStock > 0)
            .OrderBy(b => b.ExpiryDate) // FEFO: First Expired First Out
            .ToListAsync();

        ViewBag.Batches = batches.GroupBy(b => b.MedicineId).ToDictionary(g => g.Key, g => g.ToList());

        return View(prescription);
    }

    // Cap phat thuoc - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dispense(int id, string note)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PrescriptionId == id);

        if (prescription == null || prescription.Status != PrescriptionStatus.Pending)
        {
            return NotFound();
        }

        try
        {
            // Lay StaffId cua duoc si hien tai
            var pharmacistEmail = User.Identity!.Name;
            var staff = await _db.Staffs.FirstOrDefaultAsync(s => s.FullName == pharmacistEmail);
            
            if (staff == null)
            {
                TempData["Error"] = "Khong tim thay thong tin duoc si!";
                return RedirectToAction(nameof(Detail), new { id });
            }

            // Tao phieu cap phat
            var dispense = new PharmacyDispense
            {
                PrescriptionId = id,
                DispensedBy = staff.StaffId,
                DispensedAt = DateTime.UtcNow,
                Note = note,
                Items = new List<DispenseItem>()
            };

            // Cap phat tung loai thuoc
            foreach (var item in prescription.Items)
            {
                var remainingQty = item.Quantity;

                // Lay cac lo thuoc (FEFO)
                var batches = await _db.MedicineBatches
                    .Where(b => b.MedicineId == item.MedicineId
                             && b.IsActive
                             && b.ExpiryDate > DateTime.Today.AddMonths(1)
                             && b.QuantityInStock > 0)
                    .OrderBy(b => b.ExpiryDate)
                    .ToListAsync();

                foreach (var batch in batches)
                {
                    if (remainingQty <= 0) break;

                    var qtyToDispense = Math.Min(remainingQty, batch.QuantityInStock);

                    // Tao chi tiet cap phat
                    dispense.Items.Add(new DispenseItem
                    {
                        MedicineBatchId = batch.MedicineBatchId,
                        Quantity = qtyToDispense,
                        UnitPrice = batch.UnitPrice,
                        TotalPrice = qtyToDispense * batch.UnitPrice
                    });

                    // Xuat kho
                    batch.QuantityInStock -= qtyToDispense;

                    // Ghi log xuat kho
                    _db.InventoryTransactions.Add(new InventoryTransaction
                    {
                        TransactionCode = $"OUT-{DateTime.UtcNow:yyyyMMddHHmmss}",
                        MedicineBatchId = batch.MedicineBatchId,
                        Type = TransactionType.Export,
                        Quantity = -qtyToDispense,
                        TransactionDate = DateTime.UtcNow,
                        CreatedBy = staff.StaffId,
                        Note = $"Cap phat don thuoc {prescription.Code}",
                        ReferenceCode = prescription.Code
                    });

                    remainingQty -= qtyToDispense;
                }

                if (remainingQty > 0)
                {
                    TempData["Error"] = $"Khong du ton kho cho thuoc: {item.Medicine?.Name}";
                    return RedirectToAction(nameof(Detail), new { id });
                }
            }

            // C?p nh?t tr?ng thái ??n thu?c
            prescription.Status = PrescriptionStatus.Dispensed;

            _db.PharmacyDispenses.Add(dispense);
            await _db.SaveChangesAsync();

            TempData["Success"] = "C?p phát thu?c thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"L?i: {ex.Message}";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }

    // L?ch s? c?p phát
    public async Task<IActionResult> History(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? DateTime.Today.AddDays(-7);
        var to = toDate ?? DateTime.Today;

        var dispenses = await _db.PharmacyDispenses
            .Include(d => d.Prescription).ThenInclude(p => p!.Encounter).ThenInclude(e => e!.Patient)
            .Include(d => d.Pharmacist)
            .Include(d => d.Items).ThenInclude(i => i.MedicineBatch).ThenInclude(b => b!.Medicine)
            .Where(d => d.DispensedAt >= from && d.DispensedAt <= to.AddDays(1))
            .OrderByDescending(d => d.DispensedAt)
            .ToListAsync();

        ViewBag.FromDate = from;
        ViewBag.ToDate = to;

        return View(dispenses);
    }
}
