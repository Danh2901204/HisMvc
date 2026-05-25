using HisMvc.Areas.Pharmacy.Models;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Services.Workflow;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Pharmacy.Services;

public class PharmacyViewService
{
    private readonly AppDbContext _db;

    public PharmacyViewService(AppDbContext db) => _db = db;

    public async Task<PharmacyDashboardViewModel> BuildDashboardAsync()
    {
        var todayStart = DateTime.Today;
        var todayEnd = todayStart.AddDays(1);

        var vm = new PharmacyDashboardViewModel
        {
            Kpi = new PharmacyKpiViewModel
            {
                Pending = await _db.Prescriptions.CountAsync(p => p.Status == PrescriptionStatus.Pending
                    && p.Encounter != null && p.Encounter.Status == EncounterStatus.WaitingMedicine),
                DispensedToday = await _db.PharmacyDispenses.CountAsync(d => d.DispensedAt >= todayStart && d.DispensedAt < todayEnd),
                LowStock = await _db.MedicineBatches.CountAsync(b => b.IsActive && b.QuantityInStock < b.MinStockLevel),
                TotalMedicines = await _db.Medicines.CountAsync(m => m.IsActive)
            },
            PendingPrescriptions = await _db.Prescriptions
                .Include(p => p.Encounter).ThenInclude(e => e!.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.Items).ThenInclude(i => i.Medicine)
                .Where(p => p.Status == PrescriptionStatus.Pending
                    && p.Encounter != null && p.Encounter.Status == EncounterStatus.WaitingMedicine)
                .OrderBy(p => p.PrescribedAt)
                .Take(15)
                .ToListAsync(),
            LowStockBatches = await _db.MedicineBatches
                .Include(b => b.Medicine)
                .Where(b => b.IsActive && b.QuantityInStock < b.MinStockLevel)
                .OrderBy(b => b.QuantityInStock)
                .Take(10)
                .ToListAsync()
        };

        vm.Activities = await BuildActivitiesAsync(todayStart);
        return vm;
    }

    public async Task<PrescriptionListViewModel> GetPrescriptionListAsync(DateTime? date, PrescriptionStatus? status)
    {
        var targetDate = date.HasValue ? DateOnly.FromDateTime(date.Value) : DateOnly.FromDateTime(DateTime.Today);

        var query = _db.Prescriptions
            .Include(p => p.Encounter).ThenInclude(e => e!.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .Where(p => DateOnly.FromDateTime(p.PrescribedAt) == targetDate);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);
        else
            query = query.Where(p => p.Status == PrescriptionStatus.Pending
                && p.Encounter != null
                && p.Encounter.Status == EncounterStatus.WaitingMedicine);

        return new PrescriptionListViewModel
        {
            Prescriptions = await query.OrderBy(p => p.PrescribedAt).ToListAsync(),
            SelectedDate = targetDate,
            SelectedStatus = status
        };
    }

    public async Task<PrescriptionDetailViewModel?> GetPrescriptionDetailAsync(int id)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Encounter).ThenInclude(e => e!.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.PrescriptionId == id);

        if (prescription == null) return null;

        var medicineIds = prescription.Items.Select(i => i.MedicineId).ToList();
        var stocks = await _db.MedicineBatches
            .Where(b => medicineIds.Contains(b.MedicineId) && b.IsActive && b.ExpiryDate > DateTime.Today)
            .GroupBy(b => b.MedicineId)
            .Select(g => new { MedicineId = g.Key, TotalStock = g.Sum(b => b.QuantityInStock) })
            .ToDictionaryAsync(x => x.MedicineId, x => x.TotalStock);

        return new PrescriptionDetailViewModel
        {
            Prescription = prescription,
            Stocks = stocks,
            CanDispense = OutpatientWorkflowService.CanDispenseMedicine(prescription.Encounter, prescription)
        };
    }

    public async Task<DispenseViewModel?> GetDispenseFormAsync(int id)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Encounter).ThenInclude(e => e!.Patient)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.PrescriptionId == id);

        if (prescription == null || prescription.Status != PrescriptionStatus.Pending)
            return null;

        if (prescription.Encounter != null
            && !OutpatientWorkflowService.CanDispenseMedicine(prescription.Encounter, prescription))
            return null;

        var medicineIds = prescription.Items.Select(i => i.MedicineId).ToList();
        var batches = await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Where(b => medicineIds.Contains(b.MedicineId)
                && b.IsActive
                && b.ExpiryDate > DateTime.Today.AddMonths(1)
                && b.QuantityInStock > 0)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        return new DispenseViewModel
        {
            Prescription = prescription,
            Batches = batches.GroupBy(b => b.MedicineId).ToDictionary(g => g.Key, g => g.ToList())
        };
    }

    public async Task<DispenseHistoryViewModel> GetDispenseHistoryAsync(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? DateTime.Today.AddDays(-7);
        var to = toDate ?? DateTime.Today;

        return new DispenseHistoryViewModel
        {
            FromDate = from,
            ToDate = to,
            Dispenses = await _db.PharmacyDispenses
                .Include(d => d.Prescription).ThenInclude(p => p!.Encounter).ThenInclude(e => e!.Patient)
                .Include(d => d.Pharmacist)
                .Include(d => d.Items).ThenInclude(i => i.MedicineBatch).ThenInclude(b => b!.Medicine)
                .Where(d => d.DispensedAt >= from && d.DispensedAt <= to.AddDays(1))
                .OrderByDescending(d => d.DispensedAt)
                .ToListAsync()
        };
    }

    private async Task<List<DashboardActivity>> BuildActivitiesAsync(DateTime todayStart)
    {
        var activities = new List<DashboardActivity>();
        var newPres = await _db.Prescriptions
            .Include(p => p.Encounter).ThenInclude(e => e!.Patient)
            .Include(p => p.Doctor)
            .Where(p => p.PrescribedAt >= todayStart.AddDays(-1))
            .OrderByDescending(p => p.PrescribedAt).Take(10).ToListAsync();

        foreach (var p in newPres)
        {
            activities.Add(new DashboardActivity
            {
                At = p.PrescribedAt,
                Icon = p.Status == PrescriptionStatus.Dispensed ? "bi-check2-circle" : "bi-capsule",
                Title = $"Don thuốc - {p.Encounter?.Patient?.FullName}",
                Detail = $"BS {p.Doctor?.FullName}",
                Url = $"/Pharmacy/Home/Detail/{p.PrescriptionId}",
                Tag = p.Status.ToString(),
                Priority = p.Status == PrescriptionStatus.Pending ? "warning" : "success"
            });
        }

        return activities.OrderByDescending(x => x.At).Take(20).ToList();
    }

    public async Task<MedicineListViewModel> GetMedicineListAsync(string? search, bool? active)
    {
        var query = _db.Medicines.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(m => m.Name.Contains(search) || m.Code.Contains(search));

        if (active.HasValue)
            query = query.Where(m => m.IsActive == active.Value);

        return new MedicineListViewModel
        {
            Medicines = await query.OrderBy(m => m.Name).ToListAsync(),
            Search = search,
            Active = active
        };
    }
}
