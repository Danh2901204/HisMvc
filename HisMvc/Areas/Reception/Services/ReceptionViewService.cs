using HisMvc.Areas.Reception.Models;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Reception.Services;

public class ReceptionViewService
{
    private readonly AppDbContext _db;

    public ReceptionViewService(AppDbContext db) => _db = db;

    public async Task<ReceptionDashboardViewModel> BuildDashboardAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayStart = DateTime.Today;
        var todayEnd = DateTime.Today.AddDays(1);

        var vm = new ReceptionDashboardViewModel
        {
            Kpi = new ReceptionKpiViewModel
            {
                BookedToday = await _db.Appointments.CountAsync(a => a.Date == today && a.Status == AppointmentStatus.Booked),
                CheckedInToday = await _db.Encounters.CountAsync(e => e.CheckInAt >= todayStart && e.CheckInAt < todayEnd),
                CancelledToday = await _db.Appointments.CountAsync(a => a.Date == today && a.Status == AppointmentStatus.Cancelled),
                NewPatients = await _db.Patients.CountAsync(),
                BookedNext7 = await _db.Appointments.CountAsync(a => a.Date > today && a.Date <= today.AddDays(7))
            },
            UpcomingToday = await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.TimeSlot)
                .Where(a => a.Date == today && a.Status == AppointmentStatus.Booked)
                .OrderBy(a => a.TimeSlot!.Start)
                .Take(10)
                .ToListAsync()
        };

        vm.Activities = await BuildActivitiesAsync(todayStart);
        return vm;
    }

    public async Task<AppointmentListViewModel> GetAppointmentListAsync(DateOnly? date, string status)
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.Today);

        var query = _db.Appointments
            .Include(x => x.Patient)
            .Include(x => x.Department)
            .Include(x => x.Doctor)
            .Include(x => x.TimeSlot)
            .Where(x => x.Date == d);

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "Booked")
                query = query.Where(x => x.Status == AppointmentStatus.Booked);
            else if (status == "CheckedIn")
            {
                var checkedInIds = await _db.Encounters
                    .Where(e => e.Appointment!.Date == d)
                    .Select(e => e.AppointmentId)
                    .ToListAsync();
                query = query.Where(x => checkedInIds.Contains(x.AppointmentId));
            }
            else if (status == "Cancelled")
                query = query.Where(x => x.Status == AppointmentStatus.Cancelled);
        }

        var checkedInAppointments = await _db.Encounters
            .Where(e => e.Appointment!.Date == d && e.AppointmentId.HasValue)
            .Select(e => e.AppointmentId!.Value)
            .ToListAsync();

        return new AppointmentListViewModel
        {
            Appointments = await query.OrderBy(x => x.TimeSlot!.Start).ToListAsync(),
            SelectedDate = d,
            CurrentStatus = status,
            CheckedInAppointmentIds = checkedInAppointments
        };
    }

    private async Task<List<DashboardActivity>> BuildActivitiesAsync(DateTime todayStart)
    {
        var activities = new List<DashboardActivity>();

        var apps = await _db.Appointments
            .Include(a => a.Patient).Include(a => a.Doctor).Include(a => a.Department).Include(a => a.TimeSlot)
            .OrderByDescending(a => a.CreatedAt).Take(10).ToListAsync();

        foreach (var a in apps)
        {
            var diff = (DateTime.UtcNow - a.CreatedAt.ToUniversalTime()).TotalHours;
            if (diff > 48) continue;
            activities.Add(new DashboardActivity
            {
                At = a.CreatedAt,
                Icon = a.Status == AppointmentStatus.Cancelled ? "bi-calendar-x" : "bi-calendar-plus",
                Title = $"Lịch hẹn {a.Code} - {a.Patient?.FullName}",
                Detail = $"{a.Department?.Name} · BS {a.Doctor?.FullName} · {a.Date:dd/MM} {a.TimeSlot?.Start:HH:mm}",
                Url = $"/Reception/Home/Index?date={a.Date:yyyy-MM-dd}",
                Tag = a.Status == AppointmentStatus.Cancelled ? "Đã huy" : "Đã dat",
                Priority = a.Status == AppointmentStatus.Cancelled ? "high" : ""
            });
        }

        var encs = await _db.Encounters
            .Include(e => e.Patient).Include(e => e.Doctor)
            .Where(e => e.CheckInAt >= todayStart)
            .OrderByDescending(e => e.CheckInAt).Take(10).ToListAsync();

        foreach (var e in encs)
        {
            activities.Add(new DashboardActivity
            {
                At = e.CheckInAt,
                Icon = "bi-box-arrow-in-right",
                Title = $"Check-in - {e.Patient?.FullName}",
                Detail = $"Den kham BS {e.Doctor?.FullName}",
                Url = $"/Doctor/Home/Examine/{e.EncounterId}",
                Tag = "Check-in",
                Priority = "success"
            });
        }

        return activities.OrderByDescending(x => x.At).Take(25).ToList();
    }
}
