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
public class StaffController : Controller
{
    private readonly AppDbContext _db;
    private readonly AdminViewService _views;

    public StaffController(AppDbContext db, AdminViewService views)
    {
        _db = db;
        _views = views;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _views.GetStaffListAsync();
        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var model = await _views.GetStaffFormAsync();
        return View(model!);
    }

    [HttpPost]
    public async Task<IActionResult> Create([Bind(Prefix = "Staff")] Staff model)
    {
        if (!ModelState.IsValid)
        {
            var form = await _views.GetStaffFormAsync();
            form.Staff = model;
            return View(form);
        }

        _db.Staffs.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm nhân viên thành công!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _views.GetStaffFormAsync(id);
        if (model == null)
            return NotFound();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit([Bind(Prefix = "Staff")] Staff model)
    {
        if (!ModelState.IsValid)
        {
            var form = await _views.GetStaffFormAsync(model.StaffId);
            form.Staff = model;
            return View(form);
        }

        _db.Staffs.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật nhân viên thành công!";
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
            TempData["Success"] = "Xoa nhân viên thành công!";
        }
        catch
        {
            TempData["Error"] = "Không thể xóa vi có dữ liệu liên quan!";
        }

        return RedirectToAction(nameof(Index));
    }
}
