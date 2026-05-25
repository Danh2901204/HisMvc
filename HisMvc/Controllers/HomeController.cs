using System.Diagnostics;
using HisMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace HisMvc.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(AppRoles.ADMIN))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            if (User.IsInRole(AppRoles.RECEPTION))
                return RedirectToAction("Dashboard", "Home", new { area = "Reception" });
            if (User.IsInRole(AppRoles.DOCTOR))
                return RedirectToAction("Dashboard", "Home", new { area = "Doctor" });
            if (User.IsInRole(AppRoles.LAB_TECH))
                return RedirectToAction("Dashboard", "Home", new { area = "Lab" });
            if (User.IsInRole(AppRoles.PHARMACIST))
                return RedirectToAction("Dashboard", "Home", new { area = "Pharmacy" });
            if (User.IsInRole(AppRoles.CASHIER))
                return RedirectToAction("Dashboard", "Home", new { area = "Cashier" });
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
