using HisMvc.Areas.Admin.Models;
using HisMvc.Areas.Admin.Services;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.ADMIN)]
public class TimeSlotController : Controller
{
    private readonly AppDbContext _db;
    private readonly AdminViewService _views;

    public TimeSlotController(AppDbContext db, AdminViewService views)
    {
        _db = db;
        _views = views;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _views.GetTimeSlotListAsync();
        return View(model);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(TimeSlot model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _db.TimeSlots.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm khung giờ thành công!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var slot = await _db.TimeSlots.FindAsync(id);
        if (slot == null)
            return NotFound();
        return View(slot);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TimeSlot model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _db.TimeSlots.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật khung giờ thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var slot = await _db.TimeSlots.FindAsync(id);
        if (slot == null)
            return NotFound();

        try
        {
            _db.TimeSlots.Remove(slot);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Xoa khung giờ thành công!";
        }
        catch
        {
            TempData["Error"] = "Không thể xóa vi có dữ liệu liên quan!";
        }

        return RedirectToAction(nameof(Index));
    }
}
