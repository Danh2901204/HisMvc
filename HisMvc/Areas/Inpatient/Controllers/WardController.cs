using HisMvc.Areas.Inpatient.Models;
using HisMvc.Areas.Inpatient.Services;
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
    private readonly InpatientViewService _views;

    public WardController(AppDbContext db, InpatientViewService views)
    {
        _db = db;
        _views = views;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _views.GetWardListAsync();
        return View(model);
    }

    public async Task<IActionResult> BedMap(int id)
    {
        var model = await _views.GetBedMapAsync(id);
        if (model == null) return NotFound();
        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var model = await _views.GetWardFormAsync();
        return View(model!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Ward")] Ward ward)
    {
        if (ModelState.IsValid)
        {
            if (await _db.Wards.AnyAsync(w => w.Code == ward.Code))
            {
                ModelState.AddModelError("Code", "Mã buong da tồn tại!");
                var form = await _views.GetWardFormAsync();
                form.Ward = ward;
                return View(form);
            }

            _db.Wards.Add(ward);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thêm buong benh thành công!";
            return RedirectToAction(nameof(Index));
        }

        var invalidForm = await _views.GetWardFormAsync();
        invalidForm.Ward = ward;
        return View(invalidForm);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _views.GetWardFormAsync(id);
        if (model == null)
            return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Ward")] Ward ward)
    {
        if (id != ward.WardId) return NotFound();

        if (ModelState.IsValid)
        {
            if (await _db.Wards.AnyAsync(w => w.Code == ward.Code && w.WardId != id))
            {
                ModelState.AddModelError("Code", "Mã buong da tồn tại!");
                var form = await _views.GetWardFormAsync(id);
                form.Ward = ward;
                return View(form);
            }

            try
            {
                _db.Update(ward);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Cập nhật buong benh thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Wards.AnyAsync(w => w.WardId == id))
                    return NotFound();
                throw;
            }
        }

        var invalidForm = await _views.GetWardFormAsync(id);
        invalidForm.Ward = ward;
        return View(invalidForm);
    }

    public async Task<IActionResult> AddBed(int wardId)
    {
        var model = await _views.GetAddBedFormAsync(wardId);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBed([Bind(Prefix = "Bed")] Bed bed)
    {
        if (ModelState.IsValid)
        {
            if (await _db.Beds.AnyAsync(b => b.WardId == bed.WardId && b.BedNumber == bed.BedNumber))
            {
                ModelState.AddModelError("BedNumber", "So giường da tồn tại trong buong nay!");
                var form = await _views.GetAddBedFormAsync(bed.WardId);
                if (form == null) return NotFound();
                form.Bed = bed;
                return View(form);
            }

            _db.Beds.Add(bed);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thêm giường thành công!";
            return RedirectToAction(nameof(BedMap), new { id = bed.WardId });
        }

        var invalidForm = await _views.GetAddBedFormAsync(bed.WardId);
        if (invalidForm == null) return NotFound();
        invalidForm.Bed = bed;
        return View(invalidForm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBedStatus(int bedId, BedStatus status)
    {
        var bed = await _db.Beds.FindAsync(bedId);
        if (bed == null)
            return Json(new { success = false, message = "Không tìm thay giường!" });

        if (status == BedStatus.Occupied)
            return Json(new { success = false, message = "Không thể doi sang trạng thái Occupied thi cung!" });

        bed.Status = status;
        await _db.SaveChangesAsync();

        return Json(new { success = true, message = "Đã cập nhật trạng thái giường!" });
    }
}
