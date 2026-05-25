using HisMvc.Areas.Doctor.Models;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Doctor.Services;

/// <summary>
/// Doc du lieu cho View (Doctor Area).
/// </summary>
public class DoctorViewService
{
    private readonly AppDbContext _db;

    public DoctorViewService(AppDbContext db) => _db = db;

    public async Task<DoctorDashboardViewModel> BuildDashboardAsync()
    {
        var todayStart = DateTime.Today;

        var vm = new DoctorDashboardViewModel
        {
            Kpi = new DoctorKpiViewModel
            {
                Waiting = await _db.Encounters.CountAsync(e => e.Status == EncounterStatus.WaitingExam),
                InProgress = await _db.Encounters.CountAsync(e => e.Status == EncounterStatus.InService),
                WaitingResult = await _db.Encounters.CountAsync(e => e.Status == EncounterStatus.WaitingResult),
                DoneToday = await _db.Encounters.CountAsync(e =>
                    e.Status == EncounterStatus.Completed && e.EndAt >= todayStart),
                MyAdmissions = await _db.Admissions.CountAsync(a => a.Status == AdmissionStatus.Active),
                PendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Requested)
            },
            Queue = await _db.Encounters
                .Include(e => e.Patient).Include(e => e.Doctor)
                .Where(e => e.Status == EncounterStatus.WaitingExam
                    || e.Status == EncounterStatus.InService
                    || e.Status == EncounterStatus.WaitingResult)
                .OrderBy(e => e.QueueNumber ?? int.MaxValue)
                .ThenBy(e => e.CheckInAt)
                .Take(20)
                .ToListAsync()
        };

        vm.Activities = await BuildActivitiesAsync(todayStart);
        return vm;
    }

    public async Task<EncounterListViewModel> GetEncounterListAsync(string status)
    {
        var query = _db.Encounters
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .Include(x => x.Appointment)
            .AsQueryable();

        if (status == "CheckedIn")
            query = query.Where(x => x.Status == EncounterStatus.CheckedIn);
        else if (status == "InService")
            query = query.Where(x => x.Status == EncounterStatus.InService);
        else if (status == "Completed")
            query = query.Where(x => x.Status == EncounterStatus.Completed);

        return new EncounterListViewModel
        {
            Encounters = await query.OrderByDescending(x => x.CheckInAt).ToListAsync(),
            CurrentStatus = status
        };
    }

    public async Task<ExamineViewModel?> GetExamineAsync(int encounterId)
    {
        var enc = await _db.Encounters
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .Include(x => x.Appointment)
            .FirstOrDefaultAsync(x => x.EncounterId == encounterId);

        if (enc == null) return null;

        var services = await _db.Services.OrderBy(s => s.Type).ThenBy(s => s.Name).ToListAsync();
        var medicines = await _db.Medicines.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync();

        return new ExamineViewModel
        {
            Encounter = enc,
            Orders = await _db.Orders
                .Include(o => o.Service)
                .Include(o => o.OrderResult)
                .Where(o => o.EncounterId == encounterId)
                .OrderByDescending(o => o.OrderedAt)
                .ToListAsync(),
            Prescription = await _db.Prescriptions
                .Include(p => p.Items).ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.EncounterId == encounterId),
            Allergies = await _db.Allergies
                .Where(a => a.PatientId == enc.PatientId && a.IsActive)
                .OrderByDescending(a => a.Severity)
                .ToListAsync(),
            MedicalHistories = await _db.MedicalHistories
                .Where(h => h.PatientId == enc.PatientId && h.IsActive)
                .OrderByDescending(h => h.DiagnosedDate)
                .ToListAsync(),
            PreviousEncounters = await _db.Encounters
                .Include(e => e.Doctor)
                .Where(e => e.PatientId == enc.PatientId
                    && e.EncounterId != encounterId
                    && e.Status == EncounterStatus.Completed)
                .OrderByDescending(e => e.EndAt)
                .Take(5)
                .ToListAsync(),
            Services = new SelectList(services, "ServiceId", "Name"),
            Medicines = new SelectList(medicines, "MedicineId", "Name")
        };
    }

    private async Task<List<DashboardActivity>> BuildActivitiesAsync(DateTime todayStart)
    {
        var activities = new List<DashboardActivity>();

        var newCheckIns = await _db.Encounters
            .Include(e => e.Patient).Include(e => e.Doctor)
            .Where(e => e.CheckInAt >= todayStart)
            .OrderByDescending(e => e.CheckInAt).Take(8).ToListAsync();

        foreach (var e in newCheckIns)
        {
            activities.Add(new DashboardActivity
            {
                At = e.CheckInAt,
                Icon = "bi-box-arrow-in-right",
                Title = $"BN moi - {e.Patient?.FullName}",
                Detail = $"Trạng thái: {e.Status}",
                Url = $"/Doctor/Home/Examine/{e.EncounterId}",
                Tag = e.Status == EncounterStatus.CheckedIn ? "Chờ kham" : "Đang kham",
                Priority = e.Status == EncounterStatus.CheckedIn ? "warning" : ""
            });
        }

        var newResults = await _db.OrderResults
            .Include(or => or.Order).ThenInclude(o => o!.Encounter).ThenInclude(e => e!.Patient)
            .Include(or => or.Order).ThenInclude(o => o!.Service)
            .Where(or => or.ResultedAt >= todayStart.AddDays(-1))
            .OrderByDescending(or => or.ResultedAt).Take(8).ToListAsync();

        foreach (var r in newResults)
        {
            activities.Add(new DashboardActivity
            {
                At = r.ResultedAt,
                Icon = "bi-clipboard-check",
                Title = $"KQ {r.Order?.Service?.Name}",
                Detail = $"{r.Order?.Encounter?.Patient?.FullName}",
                Url = $"/Doctor/Home/Examine/{r.Order?.EncounterId}",
                Tag = "Co KQ",
                Priority = "success"
            });
        }

        return activities.OrderByDescending(x => x.At).Take(20).ToList();
    }
}
