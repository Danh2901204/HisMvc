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
public class HomeController : Controller
{
    private readonly AdminViewService _views;

    public HomeController(AdminViewService views) => _views = views;

    public async Task<IActionResult> Index()
    {
        var model = await _views.BuildDashboardAsync();
        return View(model);
    }
}
