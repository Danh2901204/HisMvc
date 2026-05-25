using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Pharmacy.Controllers;

[Area("Pharmacy")]
[Authorize(Roles = AppRoles.PHARMACIST + "," + AppRoles.ADMIN)]
public class MedicineController : Controller
{
    private readonly AppDbContext _db;

    public MedicineController(AppDbContext db)
    {
        _db = db;
    }

    // Danh sách thu?c
    public async Task<IActionResult> Index(string? search, bool? active)
    {
        var query = _db.Medicines.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => m.Name.Contains(search) || m.Code.Contains(search));
        }

        if (active.HasValue)
        {
            query = query.Where(m => m.IsActive == active.Value);
        }

        var medicines = await query.OrderBy(m => m.Name).ToListAsync();

        ViewBag.Search = search;
        ViewBag.Active = active;

        return View(medicines);
    }

    // Thêm thu?c m?i - GET
    public IActionResult Create()
    {
        return View();
    }

    // Thêm thu?c m?i - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Medicine medicine)
    {
        if (ModelState.IsValid)
        {
            // Ki?m tra trùng mã
            if (await _db.Medicines.AnyAsync(m => m.Code == medicine.Code))
            {
                ModelState.AddModelError("Code", "Mã thu?c ?ã t?n t?i!");
                return View(medicine);
            }

            _db.Medicines.Add(medicine);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thêm thu?c thành công!";
            return RedirectToAction(nameof(Index));
        }

        return View(medicine);
    }

    // S?a thông tin thu?c - GET
    public async Task<IActionResult> Edit(int id)
    {
        var medicine = await _db.Medicines.FindAsync(id);
        if (medicine == null) return NotFound();

        return View(medicine);
    }

    // S?a thông tin thu?c - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Medicine medicine)
    {
        if (id != medicine.MedicineId) return NotFound();

        if (ModelState.IsValid)
        {
            // Ki?m tra trùng mã (tr? chính nó)
            if (await _db.Medicines.AnyAsync(m => m.Code == medicine.Code && m.MedicineId != id))
            {
                ModelState.AddModelError("Code", "Mã thu?c ?ã t?n t?i!");
                return View(medicine);
            }

            try
            {
                _db.Update(medicine);
                await _db.SaveChangesAsync();

                TempData["Success"] = "C?p nh?t thu?c thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Medicines.AnyAsync(m => m.MedicineId == id))
                    return NotFound();
                throw;
            }
        }

        return View(medicine);
    }

    // Xem ton kho theo thuoc
    public async Task<IActionResult> Stock(int id)
    {
        var medicine = await _db.Medicines.FindAsync(id);
        if (medicine == null) return NotFound();

        var batches = await _db.MedicineBatches
            .Where(b => b.MedicineId == id)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        ViewBag.Medicine = medicine;
        ViewBag.TotalStock = batches.Where(b => b.IsActive).Sum(b => b.QuantityInStock);

        return View(batches);
    }

    // Them lo thuoc moi - GET
    public async Task<IActionResult> AddBatch(int medicineId)
    {
        var medicine = await _db.Medicines.FindAsync(medicineId);
        if (medicine == null) return NotFound();

        ViewBag.Medicine = medicine;

        var batch = new MedicineBatch
        {
            MedicineId = medicineId,
            ManufactureDate = DateTime.Today,
            ExpiryDate = DateTime.Today.AddYears(2)
        };

        return View(batch);
    }

    // Thêm lô thu?c m?i - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBatch(MedicineBatch batch)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // L?y thông tin Staff
                var pharmacistEmail = User.Identity!.Name;
                var staff = await _db.Staffs.FirstOrDefaultAsync(s => s.FullName == pharmacistEmail);

                _db.MedicineBatches.Add(batch);
                await _db.SaveChangesAsync();

                // Ghi log nh?p kho
                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    TransactionCode = $"IN-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    MedicineBatchId = batch.MedicineBatchId,
                    Type = TransactionType.Import,
                    Quantity = batch.QuantityInStock,
                    TransactionDate = DateTime.UtcNow,
                    CreatedBy = staff?.StaffId,
                    Note = $"Nh?p lô thu?c m?i: {batch.BatchNumber}"
                });

                await _db.SaveChangesAsync();

                TempData["Success"] = "Thêm lô thu?c thành công!";
                return RedirectToAction(nameof(Stock), new { id = batch.MedicineId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"L?i: {ex.Message}");
            }
        }

        var medicine = await _db.Medicines.FindAsync(batch.MedicineId);
        ViewBag.Medicine = medicine;

        return View(batch);
    }

    // C?nh báo t?n kho th?p
    public async Task<IActionResult> LowStock()
    {
        var lowStockItems = await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Where(b => b.IsActive && b.QuantityInStock <= b.MinStockLevel)
            .OrderBy(b => b.QuantityInStock)
            .ToListAsync();

        return View(lowStockItems);
    }

    // C?nh báo s?p h?t h?n
    public async Task<IActionResult> Expiring()
    {
        var expiringBatches = await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Where(b => b.IsActive 
                     && b.ExpiryDate > DateTime.Today 
                     && b.ExpiryDate <= DateTime.Today.AddMonths(3)) // S?p h?t h?n trong 3 tháng
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        return View(expiringBatches);
    }
}
