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

    public TimeSlotController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var slots = await _db.TimeSlots.OrderBy(x => x.Start).ToListAsync();
        return View(slots);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(TimeSlot model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _db.TimeSlots.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Them khung gio thanh cong!";
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
        TempData["Success"] = "Cap nhat khung gio thanh cong!";
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
            TempData["Success"] = "Xoa khung gio thanh cong!";
        }
        catch
        {
            TempData["Error"] = "Khong the xoa vi co du lieu lien quan!";
        }

        return RedirectToAction(nameof(Index));
    }
}
