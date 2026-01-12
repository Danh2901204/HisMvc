using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.ADMIN)]
public class ServiceController : Controller
{
    private readonly AppDbContext _db;

    public ServiceController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var services = await _db.Services.OrderBy(x => x.Type).ThenBy(x => x.Name).ToListAsync();
        return View(services);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Service model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _db.Services.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Them dich vu thanh cong!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var service = await _db.Services.FindAsync(id);
        if (service == null)
            return NotFound();
        return View(service);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Service model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _db.Services.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cap nhat dich vu thanh cong!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _db.Services.FindAsync(id);
        if (service == null)
            return NotFound();

        try
        {
            _db.Services.Remove(service);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Xoa dich vu thanh cong!";
        }
        catch
        {
            TempData["Error"] = "Khong the xoa vi co du lieu lien quan!";
        }

        return RedirectToAction(nameof(Index));
    }
}
