using HisMvc.Areas.Admin.Models;
using HisMvc.Areas.Admin.Services;
using HisMvc.Data;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.ADMIN)]
public class UserController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AdminViewService _views;

    public UserController(UserManager<AppUser> userManager, AdminViewService views)
    {
        _userManager = userManager;
        _views = views;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _views.GetUserListAsync();
        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var model = await _views.GetUserCreateFormAsync();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string userName, string email, string password, string role, int? staffId)
    {
        var user = new AppUser
        {
            UserName = userName,
            Email = email,
            StaffId = staffId
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            var form = await _views.GetUserCreateFormAsync();
            return View(form);
        }

        if (!string.IsNullOrEmpty(role))
            await _userManager.AddToRoleAsync(user, role);

        TempData["Success"] = "Tạo tài khoản thành công!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var model = await _views.GetUserEditFormAsync(id);
        if (model == null)
            return NotFound();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, string email, string role, int? staffId)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.Email = email;
        user.StaffId = staffId;
        await _userManager.UpdateAsync(user);

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!string.IsNullOrEmpty(role))
            await _userManager.AddToRoleAsync(user, role);

        TempData["Success"] = "Cập nhật tài khoản thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        await _userManager.DeleteAsync(user);
        TempData["Success"] = "Xoa tài khoản thành công!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(string id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(user);
        }

        TempData["Success"] = "Dat lai mat khau thành công!";
        return RedirectToAction(nameof(Index));
    }
}
