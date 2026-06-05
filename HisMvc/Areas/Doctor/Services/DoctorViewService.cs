using HisMvc.Areas.Doctor.Models;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Doctor.Services;

public class DoctorViewService
{
    private readonly AppDbContext _db;

    public DoctorViewService(AppDbContext db) => _db = db;

    public async Task<DoctorDashboardViewModel> BuildDashboardAsync()
    {
        var todayEncounters = EncounterDayHelper.WhereCheckInToday(_db.Encounters);
        var (todayStartUtc, todayEndUtc) = EncounterDayHelper.GetLocalTodayUtcRange();

        var vm = new DoctorDashboardViewModel
        {
            Kpi = new DoctorKpiViewModel
            {
                Waiting = await todayEncounters.CountAsync(e => e.Status == EncounterStatus.WaitingExam),
                InProgress = await todayEncounters.CountAsync(e => e.Status == EncounterStatus.InService),
                WaitingResult = await todayEncounters.CountAsync(e => e.Status == EncounterStatus.WaitingResult),
                DoneToday = await todayEncounters.CountAsync(e => e.Status == EncounterStatus.Completed),
                MyAdmissions = await _db.Admissions.CountAsync(a => a.Status == AdmissionStatus.Active),
                PendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Requested)
            },
            Queue = await todayEncounters
                .Include(e => e.Patient).Include(e => e.Doctor)
                .Where(e => e.Status == EncounterStatus.WaitingExam
                    || e.Status == EncounterStatus.InService
                    || e.Status == EncounterStatus.WaitingResult)
                .OrderBy(e => e.QueueNumber ?? int.MaxValue)
                .ThenBy(e => e.CheckInAt)
                .Take(20)
                .ToListAsync()
        };

        vm.Activities = await BuildActivitiesAsync(todayStartUtc, todayEndUtc);
        return vm;
    }

    public async Task<EncounterHistoryViewModel> GetExamHistoryAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        string search,
        int? doctorId = null)
    {
        var to = toDate ?? DateOnly.FromDateTime(DateTime.Today);
        var from = fromDate ?? to.AddDays(-30);
        if (from > to)
            (from, to) = (to, from);

        var (fromUtc, toUtc) = EncounterDayHelper.GetLocalDateUtcRange(from, to);

        var query = _db.Encounters
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .Include(x => x.Department)
            .Where(x => x.Status == EncounterStatus.Completed || x.Status == EncounterStatus.Cancelled)
            .Where(x => x.CheckInAt >= fromUtc && x.CheckInAt < toUtc);

        if (doctorId.HasValue)
            query = query.Where(x => x.DoctorId == doctorId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                (x.Patient != null && (x.Patient.FullName.Contains(term) || x.Patient.Phone.Contains(term)))
                || x.EncounterCode.Contains(term)
                || (x.Diagnosis != null && x.Diagnosis.Contains(term)));
        }

        return new EncounterHistoryViewModel
        {
            Encounters = await query.OrderByDescending(x => x.EndAt).ThenByDescending(x => x.CheckInAt).ToListAsync(),
            FromDate = from,
            ToDate = to,
            Search = search ?? ""
        };
    }

    public async Task<EncounterListViewModel> GetEncounterListAsync(string status)
    {
        var query = _db.Encounters
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .Include(x => x.Appointment)
            .AsQueryable();

        if (status == "Completed")
        {
            var (cutoffUtc, _) = EncounterDayHelper.GetLocalDateUtcRange(
                DateOnly.FromDateTime(DateTime.Today.AddDays(-90)),
                DateOnly.FromDateTime(DateTime.Today));

            query = query
                .Where(x => x.Status == EncounterStatus.Completed && x.CheckInAt >= cutoffUtc);
        }
        else if (status == "CheckedIn")
        {
            query = EncounterDayHelper.WhereCheckInToday(query)
                .Where(x => x.Status == EncounterStatus.CheckedIn);
        }
        else if (status == "InService")
        {
            query = EncounterDayHelper.WhereCheckInToday(query)
                .Where(x => x.Status == EncounterStatus.InService);
        }
        else
        {
            query = EncounterDayHelper.WhereCheckInToday(query)
                .Where(x => x.Status != EncounterStatus.Completed && x.Status != EncounterStatus.Cancelled);
        }

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

    private async Task<List<DashboardActivity>> BuildActivitiesAsync(DateTime todayStartUtc, DateTime todayEndUtc)
    {
        var activities = new List<DashboardActivity>();

        var newCheckIns = await _db.Encounters
            .Include(e => e.Patient).Include(e => e.Doctor)
            .Where(e => e.CheckInAt >= todayStartUtc && e.CheckInAt < todayEndUtc)
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
            .Where(or => or.ResultedAt >= todayStartUtc.AddDays(-1))
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
