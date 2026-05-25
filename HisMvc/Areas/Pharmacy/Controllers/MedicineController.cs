using HisMvc.Areas.Pharmacy.Models;
using HisMvc.Areas.Pharmacy.Services;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Pharmacy.Controllers;

[Area("Pharmacy")]
[Authorize(Roles = AppRoles.PHARMACIST + "," + AppRoles.ADMIN)]
public class MedicineController : Controller
{
    private readonly AppDbContext _db;
    private readonly PharmacyViewService _views;
    private readonly CurrentStaffService _staffService;

    public MedicineController(AppDbContext db, PharmacyViewService views, CurrentStaffService staffService)
    {
        _db = db;
        _views = views;
        _staffService = staffService;
    }

    public async Task<IActionResult> Index(string? search, bool? active)
    {
        var model = await _views.GetMedicineListAsync(search, active);
        return View(model);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Medicine medicine)
    {
        if (ModelState.IsValid)
        {
            if (await _db.Medicines.AnyAsync(m => m.Code == medicine.Code))
            {
                ModelState.AddModelError("Code", "Mã thuốc đã tồn tại!");
                return View(medicine);
            }

            _db.Medicines.Add(medicine);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thêm thuốc thành công!";
            return RedirectToAction(nameof(Index));
        }

        return View(medicine);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var medicine = await _db.Medicines.FindAsync(id);
        if (medicine == null) return NotFound();
        return View(medicine);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Medicine medicine)
    {
        if (id != medicine.MedicineId) return NotFound();

        if (ModelState.IsValid)
        {
            if (await _db.Medicines.AnyAsync(m => m.Code == medicine.Code && m.MedicineId != id))
            {
                ModelState.AddModelError("Code", "Mã thuốc đã tồn tại!");
                return View(medicine);
            }

            try
            {
                _db.Update(medicine);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thuốc thành công!";
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBatch(MedicineBatch batch)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var staffId = await _staffService.TryGetStaffIdAsync(User);

                _db.MedicineBatches.Add(batch);
                await _db.SaveChangesAsync();

                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    TransactionCode = $"IN-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    MedicineBatchId = batch.MedicineBatchId,
                    Type = TransactionType.Import,
                    Quantity = batch.QuantityInStock,
                    TransactionDate = DateTime.UtcNow,
                    CreatedBy = staffId,
                    StaffId = staffId,
                    Note = $"Nhập lô thuốc mới: {batch.BatchNumber}"
                });

                await _db.SaveChangesAsync();

                TempData["Success"] = "Thêm lô thuốc thành công!";
                return RedirectToAction(nameof(Stock), new { id = batch.MedicineId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            }
        }

        var medicine = await _db.Medicines.FindAsync(batch.MedicineId);
        ViewBag.Medicine = medicine;

        return View(batch);
    }

    public async Task<IActionResult> LowStock()
    {
        var lowStockItems = await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Where(b => b.IsActive && b.QuantityInStock <= b.MinStockLevel)
            .OrderBy(b => b.QuantityInStock)
            .ToListAsync();

        return View(lowStockItems);
    }

    public async Task<IActionResult> Expiring()
    {
        var expiringBatches = await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Where(b => b.IsActive
                     && b.ExpiryDate > DateTime.Today
                     && b.ExpiryDate <= DateTime.Today.AddMonths(3))
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        return View(expiringBatches);
    }
}
