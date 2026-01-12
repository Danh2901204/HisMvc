using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.ADMIN)]
public class StaffController : Controller
{
    private readonly AppDbContext _db;

    public StaffController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var staffs = await _db.Staffs.Include(x => x.Department).OrderBy(x => x.FullName).ToListAsync();
        return View(staffs);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = new SelectList(await _db.Departments.ToListAsync(), "DepartmentId", "Name");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Staff model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = new SelectList(await _db.Departments.ToListAsync(), "DepartmentId", "Name");
            return View(model);
        }

        _db.Staffs.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Them nhan vien thanh cong!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var staff = await _db.Staffs.FindAsync(id);
        if (staff == null)
            return NotFound();

        ViewBag.Departments = new SelectList(await _db.Departments.ToListAsync(), "DepartmentId", "Name", staff.DepartmentId);
        return View(staff);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Staff model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = new SelectList(await _db.Departments.ToListAsync(), "DepartmentId", "Name", model.DepartmentId);
            return View(model);
        }

        _db.Staffs.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cap nhat nhan vien thanh cong!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var staff = await _db.Staffs.FindAsync(id);
        if (staff == null)
            return NotFound();

        try
        {
            _db.Staffs.Remove(staff);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Xoa nhan vien thanh cong!";
        }
        catch
        {
            TempData["Error"] = "Khong the xoa vi co du lieu lien quan!";
        }

        return RedirectToAction(nameof(Index));
    }
}
