using HisMvc.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HisMvc.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signIn;
    private readonly UserManager<AppUser> _users;

    public AccountController(SignInManager<AppUser> signIn, UserManager<AppUser> users)
    {
        _signIn = signIn;
        _users = users;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        email = (email ?? "").Trim();

        var user = await _users.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError("", "Sai tài khoản");
            return View();
        }

        var res = await _signIn.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
        if (!res.Succeeded)
        {
            ModelState.AddModelError("", "Sai mật khẩu");
            return View();
        }

        return Redirect(returnUrl ?? "/");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}
