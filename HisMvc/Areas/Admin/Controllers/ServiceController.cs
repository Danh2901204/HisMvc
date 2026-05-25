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
public class ServiceController : Controller
{
    private readonly AppDbContext _db;
    private readonly AdminViewService _views;

    public ServiceController(AppDbContext db, AdminViewService views)
    {
        _db = db;
        _views = views;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _views.GetServiceListAsync();
        return View(model);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(Service model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _db.Services.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm dịch vụ thành công!";
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
        TempData["Success"] = "Cập nhật dịch vụ thành công!";
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
            TempData["Success"] = "Xoa dịch vụ thành công!";
        }
        catch
        {
            TempData["Error"] = "Không thể xóa vi có dữ liệu liên quan!";
        }

        return RedirectToAction(nameof(Index));
    }
}
