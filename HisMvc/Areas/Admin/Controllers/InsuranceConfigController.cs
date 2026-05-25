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
public class InsuranceConfigController : Controller
{
    private readonly AppDbContext _db;
    private readonly AdminViewService _views;

    public InsuranceConfigController(AppDbContext db, AdminViewService views)
    {
        _db = db;
        _views = views;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _views.GetInsuranceConfigListAsync();
        return View(model);
    }

    public IActionResult Create() => View(new InsuranceConfig());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InsuranceConfig model)
    {
        model.InsuranceType = model.InsuranceType.Trim().ToUpperInvariant();

        if (await _db.InsuranceConfigs.AnyAsync(x => x.InsuranceType == model.InsuranceType))
            ModelState.AddModelError(nameof(InsuranceConfig.InsuranceType), "Loại thẻ BHYT da tồn tại!");

        if (!ModelState.IsValid)
            return View(model);

        _db.InsuranceConfigs.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm cau hinh BHYT thành công!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var config = await _db.InsuranceConfigs.FindAsync(id);
        if (config == null)
            return NotFound();
        return View(config);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(InsuranceConfig model)
    {
        model.InsuranceType = model.InsuranceType.Trim().ToUpperInvariant();

        if (await _db.InsuranceConfigs.AnyAsync(x =>
                x.InsuranceType == model.InsuranceType && x.InsuranceConfigId != model.InsuranceConfigId))
            ModelState.AddModelError(nameof(InsuranceConfig.InsuranceType), "Loại thẻ BHYT da duoc su dung boi cau hinh khac!");

        if (!ModelState.IsValid)
            return View(model);

        var config = await _db.InsuranceConfigs.FindAsync(model.InsuranceConfigId);
        if (config == null)
            return NotFound();

        config.InsuranceType = model.InsuranceType;
        config.Description = model.Description;
        config.DefaultCoveragePercent = model.DefaultCoveragePercent;
        config.RequireRegistration = model.RequireRegistration;
        config.IsActive = model.IsActive;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật cau hinh BHYT thành công!";
        return RedirectToAction(nameof(Index));
    }
}
