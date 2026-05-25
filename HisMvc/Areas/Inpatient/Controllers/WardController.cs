using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Inpatient.Controllers;

[Area("Inpatient")]
[Authorize(Roles = AppRoles.ADMIN)]
public class WardController : Controller
{
    private readonly AppDbContext _db;

    public WardController(AppDbContext db)
    {
        _db = db;
    }

    // Danh sach buong benh
    public async Task<IActionResult> Index()
    {
        var wards = await _db.Wards
            .Include(w => w.Department)
            .OrderBy(w => w.Name)
            .ToListAsync();

        // Dem so giuong cho moi ward
        var bedCounts = await _db.Beds
            .GroupBy(b => b.WardId)
            .Select(g => new { WardId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.WardId, x => x.Count);

        ViewBag.BedCounts = bedCounts;

        return View(wards);
    }

    // Xem so do giuong trong buong
    public async Task<IActionResult> BedMap(int id)
    {
        var ward = await _db.Wards
            .Include(w => w.Department)
            .FirstOrDefaultAsync(w => w.WardId == id);

        if (ward == null) return NotFound();

        var beds = await _db.Beds
            .Where(b => b.WardId == id)
            .OrderBy(b => b.BedNumber)
            .ToListAsync();

        // Lay thong tin benh nhan dang nam
        var occupiedBedIds = beds.Where(b => b.Status == BedStatus.Occupied).Select(b => b.BedId).ToList();
        var admissions = await _db.Admissions
            .Include(a => a.Patient)
            .Where(a => occupiedBedIds.Contains(a.BedId) && a.Status == AdmissionStatus.Active)
            .ToDictionaryAsync(a => a.BedId, a => a);

        ViewBag.Ward = ward;
        ViewBag.Beds = beds;
        ViewBag.Admissions = admissions;

        return View();
    }

    // Them buong moi - GET
    public async Task<IActionResult> Create()
    {
        var departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
        ViewBag.Departments = departments;
        return View();
    }

    // Them buong moi - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ward ward)
    {
        if (ModelState.IsValid)
        {
            if (await _db.Wards.AnyAsync(w => w.Code == ward.Code))
            {
                ModelState.AddModelError("Code", "Ma buong da ton tai!");
                var departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
                ViewBag.Departments = departments;
                return View(ward);
            }

            _db.Wards.Add(ward);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Them buong benh thanh cong!";
            return RedirectToAction(nameof(Index));
        }

        var depts = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
        ViewBag.Departments = depts;
        return View(ward);
    }

    // Sua buong - GET
    public async Task<IActionResult> Edit(int id)
    {
        var ward = await _db.Wards.FindAsync(id);
        if (ward == null) return NotFound();

        var departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
        ViewBag.Departments = departments;

        return View(ward);
    }

    // Sua buong - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Ward ward)
    {
        if (id != ward.WardId) return NotFound();

        if (ModelState.IsValid)
        {
            if (await _db.Wards.AnyAsync(w => w.Code == ward.Code && w.WardId != id))
            {
                ModelState.AddModelError("Code", "Ma buong da ton tai!");
                var departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
                ViewBag.Departments = departments;
                return View(ward);
            }

            try
            {
                _db.Update(ward);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Cap nhat buong benh thanh cong!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Wards.AnyAsync(w => w.WardId == id))
                    return NotFound();
                throw;
            }
        }

        var depts = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
        ViewBag.Departments = depts;
        return View(ward);
    }

    // Them giuong - GET
    public async Task<IActionResult> AddBed(int wardId)
    {
        var ward = await _db.Wards.FindAsync(wardId);
        if (ward == null) return NotFound();

        ViewBag.Ward = ward;

        var bed = new Bed
        {
            WardId = wardId
        };

        return View(bed);
    }

    // Them giuong - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBed(Bed bed)
    {
        if (ModelState.IsValid)
        {
            // Kiem tra trung so giuong trong cung buong
            if (await _db.Beds.AnyAsync(b => b.WardId == bed.WardId && b.BedNumber == bed.BedNumber))
            {
                ModelState.AddModelError("BedNumber", "So giuong da ton tai trong buong nay!");
                var w = await _db.Wards.FindAsync(bed.WardId);
                ViewBag.Ward = w;
                return View(bed);
            }

            _db.Beds.Add(bed);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Them giuong thanh cong!";
            return RedirectToAction(nameof(BedMap), new { id = bed.WardId });
        }

        var ward = await _db.Wards.FindAsync(bed.WardId);
        ViewBag.Ward = ward;
        return View(bed);
    }

    // Cap nhat trang thai giuong
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBedStatus(int bedId, BedStatus status)
    {
        var bed = await _db.Beds.FindAsync(bedId);
        if (bed == null)
        {
            return Json(new { success = false, message = "Khong tim thay giuong!" });
        }

        // Khong cho phep doi sang Occupied thi cung
        if (status == BedStatus.Occupied)
        {
            return Json(new { success = false, message = "Khong the doi sang trang thai Occupied thi cung!" });
        }

        bed.Status = status;
        await _db.SaveChangesAsync();

        return Json(new { success = true, message = "Da cap nhat trang thai giuong!" });
    }
}
