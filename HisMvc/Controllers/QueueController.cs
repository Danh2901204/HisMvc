using HisMvc.Data;
using HisMvc.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Controllers;

// Man hinh hien thi STT cho phong cho (Bước 3 trong luồng KCB)
// Public - không yêu cầu đăng nhập de man hinh hien thi luon hoat dong
[AllowAnonymous]
public class QueueController : Controller
{
    private readonly AppDbContext _db;

    public QueueController(AppDbContext db) => _db = db;

    // Trang hien thi tong (cho phong cho)
    public async Task<IActionResult> Index(int? departmentId)
    {
        var today = DateTime.Today;
        var todayEnd = today.AddDays(1);

        var q = _db.Encounters
            .Include(e => e.Patient)
            .Include(e => e.Doctor)
            .Include(e => e.Department)
            .Where(e => e.QueuedAt.HasValue && e.QueuedAt.Value >= today && e.QueuedAt.Value < todayEnd)
            .Where(e => e.Status == EncounterStatus.WaitingExam ||
                        e.Status == EncounterStatus.InService);

        if (departmentId.HasValue)
            q = q.Where(e => e.DepartmentId == departmentId.Value);

        var calling = await q.Where(e => e.Status == EncounterStatus.InService)
            .OrderByDescending(e => e.StartedAt)
            .Take(8)
            .ToListAsync();

        var waiting = await q.Where(e => e.Status == EncounterStatus.WaitingExam)
            .OrderBy(e => e.QueueNumber)
            .Take(20)
            .ToListAsync();

        ViewBag.Calling = calling;
        ViewBag.Waiting = waiting;
        ViewBag.Departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
        ViewBag.DepartmentId = departmentId;
        return View();
    }
}
