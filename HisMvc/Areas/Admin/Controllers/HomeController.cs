using HisMvc.Data;
using HisMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.ADMIN)]
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var stats = new
        {
            TotalDepartments = await _db.Departments.CountAsync(),
            TotalStaff = await _db.Staffs.CountAsync(),
            TotalServices = await _db.Services.CountAsync(),
            TotalPatients = await _db.Patients.CountAsync(),
            TotalAppointmentsToday = await _db.Appointments.CountAsync(x => x.Date == DateOnly.FromDateTime(DateTime.Today)),
            TotalEncountersToday = await _db.Encounters.CountAsync(x => x.CheckInAt.Date == DateTime.Today)
        };
        return View(stats);
    }
}
