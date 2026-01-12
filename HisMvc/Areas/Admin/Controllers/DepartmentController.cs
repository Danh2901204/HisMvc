using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.ADMIN)]
public class DepartmentController : Controller
{
    private readonly AppDbContext _db;

    public DepartmentController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var departments = await _db.Departments.OrderBy(x => x.Name).ToListAsync();
        return View(departments);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Department model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _db.Departments.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Them khoa/phong ban thanh cong!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dept = await _db.Departments.FindAsync(id);
        if (dept == null)
            return NotFound();
        return View(dept);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Department model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _db.Departments.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cap nhat khoa/phong ban thanh cong!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var dept = await _db.Departments.FindAsync(id);
        if (dept == null)
            return NotFound();

        try
        {
            _db.Departments.Remove(dept);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Xoa khoa/phong ban thanh cong!";
        }
        catch
        {
            TempData["Error"] = "Khong the xoa vi co du lieu lien quan!";
        }

        return RedirectToAction(nameof(Index));
    }
}
