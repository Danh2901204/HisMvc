using HisMvc.Data;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.ADMIN)]
public class UserController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _db;

    public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userRoles = new List<(AppUser User, string Roles)>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles.Add((user, string.Join(", ", roles)));
        }

        return View(userRoles);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _roleManager.Roles.Select(x => x.Name).ToListAsync();
        ViewBag.Staffs = new SelectList(await _db.Staffs.ToListAsync(), "StaffId", "FullName");
        return View();
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

            ViewBag.Roles = await _roleManager.Roles.Select(x => x.Name).ToListAsync();
            ViewBag.Staffs = new SelectList(await _db.Staffs.ToListAsync(), "StaffId", "FullName", staffId);
            return View();
        }

        if (!string.IsNullOrEmpty(role))
            await _userManager.AddToRoleAsync(user, role);

        TempData["Success"] = "Tao tai khoan thanh cong!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        ViewBag.CurrentRole = roles.FirstOrDefault();
        ViewBag.Roles = await _roleManager.Roles.Select(x => x.Name).ToListAsync();
        ViewBag.Staffs = new SelectList(await _db.Staffs.ToListAsync(), "StaffId", "FullName", user.StaffId);
        return View(user);
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

        TempData["Success"] = "Cap nhat tai khoan thanh cong!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        await _userManager.DeleteAsync(user);
        TempData["Success"] = "Xoa tai khoan thanh cong!";
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

        TempData["Success"] = "Dat lai mat khau thanh cong!";
        return RedirectToAction(nameof(Index));
    }
}
