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
public class DepartmentController : Controller
{
    private readonly AppDbContext _db;
    private readonly AdminViewService _views;

    public DepartmentController(AppDbContext db, AdminViewService views)
    {
        _db = db;
        _views = views;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _views.GetDepartmentListAsync();
        return View(model);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(Department model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _db.Departments.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm khoa/phong ban thành công!";
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
        TempData["Success"] = "Cập nhật khoa/phong ban thành công!";
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
            TempData["Success"] = "Xoa khoa/phong ban thành công!";
        }
        catch
        {
            TempData["Error"] = "Không thể xóa vi có dữ liệu liên quan!";
        }

        return RedirectToAction(nameof(Index));
    }
}
