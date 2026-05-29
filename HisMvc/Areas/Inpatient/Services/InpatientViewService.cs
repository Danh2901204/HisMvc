using HisMvc.Areas.Inpatient.Models;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Inpatient.Services;

public class InpatientViewService
{
    private readonly AppDbContext _db;

    public InpatientViewService(AppDbContext db) => _db = db;

    public async Task<InpatientDashboardViewModel> BuildDashboardAsync()
    {
        var todayStart = DateTime.Today;
        var todayEnd = todayStart.AddDays(1);

        var vm = new InpatientDashboardViewModel
        {
            Kpi = new InpatientKpiViewModel
            {
                ActiveAdmissions = await _db.Admissions.CountAsync(a => a.Status == AdmissionStatus.Active),
                AdmittedToday = await _db.Admissions.CountAsync(a => a.AdmittedAt >= todayStart && a.AdmittedAt < todayEnd),
                DischargedToday = await _db.Admissions.CountAsync(a =>
                    a.Status == AdmissionStatus.Discharged && a.DischargedAt.HasValue
                    && a.DischargedAt.Value >= todayStart && a.DischargedAt.Value < todayEnd),
                PendingOrders = await _db.MedicalOrders.CountAsync(o => o.Status == MedicalOrderStatus.Ordered),
                ActiveSurgeries = await _db.Surgeries.CountAsync(s =>
                    s.Status == SurgeryStatus.Scheduled || s.Status == SurgeryStatus.InProgress)
            },
            ActiveAdmissions = await _db.Admissions
                .Include(a => a.Patient)
                .Include(a => a.Bed).ThenInclude(b => b!.Ward)
                .Include(a => a.AttendingDoctor)
                .Where(a => a.Status == AdmissionStatus.Active)
                .OrderByDescending(a => a.AdmittedAt)
                .Take(15)
                .ToListAsync()
        };

        vm.Activities = await BuildActivitiesAsync(todayStart);
        return vm;
    }

    public async Task<AdmissionListViewModel> GetAdmissionListAsync(int? wardId, AdmissionStatus? status)
    {
        var query = _db.Admissions
            .Include(a => a.Patient)
            .Include(a => a.Bed).ThenInclude(b => b!.Ward)
            .Include(a => a.AttendingDoctor)
            .AsQueryable();

        if (wardId.HasValue)
            query = query.Where(a => a.Bed!.WardId == wardId.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        else
            query = query.Where(a => a.Status == AdmissionStatus.Active);

        return new AdmissionListViewModel
        {
            Admissions = await query.OrderByDescending(a => a.AdmittedAt).ToListAsync(),
            Wards = await _db.Wards.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(),
            SelectedWardId = wardId,
            SelectedStatus = status
        };
    }

    public async Task<AdmissionDetailViewModel?> GetAdmissionDetailAsync(int id)
    {
        var admission = await _db.Admissions
            .Include(a => a.Patient)
            .Include(a => a.Bed).ThenInclude(b => b!.Ward)
            .Include(a => a.AttendingDoctor)
            .Include(a => a.MedicalOrders).ThenInclude(o => o.OrderedByStaff)
            .FirstOrDefaultAsync(a => a.AdmissionId == id);

        if (admission == null) return null;

        var prescriptionIds = await _db.MedicalOrders
            .Where(mo => mo.AdmissionId == id && mo.PrescriptionId != null)
            .Select(mo => mo.PrescriptionId!.Value)
            .ToListAsync();

        return new AdmissionDetailViewModel
        {
            Admission = admission,
            VitalSigns = await _db.VitalSigns
                .Where(v => v.AdmissionId == id)
                .OrderByDescending(v => v.RecordedAt)
                .Take(10)
                .ToListAsync(),
            Prescriptions = await _db.Prescriptions
                .Include(p => p.Items).ThenInclude(i => i.Medicine)
                .Where(p => prescriptionIds.Contains(p.PrescriptionId))
                .ToListAsync(),
            Allergies = await _db.Allergies
                .Where(a => a.PatientId == admission.PatientId && a.IsActive)
                .OrderByDescending(a => a.Severity)
                .ToListAsync(),
            MedicalHistories = await _db.MedicalHistories
                .Where(h => h.PatientId == admission.PatientId && h.IsActive)
                .OrderByDescending(h => h.DiagnosedDate)
                .ToListAsync(),
            Surgeries = await _db.Surgeries
                .Include(s => s.Surgeon)
                .Where(s => s.AdmissionId == id)
                .OrderByDescending(s => s.ScheduledAt)
                .ToListAsync()
        };
    }

    public async Task<AdmitFormViewModel> GetAdmitFormAsync(int? patientId)
    {
        var vm = new AdmitFormViewModel
        {
            AvailableBeds = await _db.Beds
                .Include(b => b.Ward)
                .Where(b => b.IsActive && b.Status == BedStatus.Empty)
                .OrderBy(b => b.Ward!.Name).ThenBy(b => b.BedNumber)
                .ToListAsync(),
            Doctors = await _db.Staffs
                .Where(s => s.StaffType == "DOCTOR")
                .OrderBy(s => s.FullName)
                .ToListAsync()
        };

        if (patientId.HasValue)
            vm.Patient = await _db.Patients.FindAsync(patientId.Value);

        return vm;
    }

    public async Task<VitalSignFormViewModel?> GetVitalSignFormAsync(int admissionId)
    {
        var admission = await _db.Admissions
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.AdmissionId == admissionId);

        if (admission == null || admission.Status != AdmissionStatus.Active)
            return null;

        return new VitalSignFormViewModel { Admission = admission };
    }

    public async Task<MedicalOrderListViewModel?> GetMedicalOrderListAsync(int admissionId)
    {
        var admission = await _db.Admissions
            .Include(a => a.Patient)
            .Include(a => a.Bed).ThenInclude(b => b!.Ward)
            .Include(a => a.AttendingDoctor)
            .FirstOrDefaultAsync(a => a.AdmissionId == admissionId);

        if (admission == null) return null;

        return new MedicalOrderListViewModel
        {
            Admission = admission,
            Orders = await _db.MedicalOrders
                .Include(o => o.OrderedByStaff)
                .Include(o => o.ExecutedByStaff)
                .Include(o => o.Prescription).ThenInclude(p => p!.Items).ThenInclude(i => i.Medicine)
                .Where(o => o.AdmissionId == admissionId)
                .OrderByDescending(o => o.ScheduledAt)
                .ToListAsync(),
            Medicines = await _db.Medicines.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync(),
            Services = await _db.Services.OrderBy(s => s.Type).ThenBy(s => s.Name).ToListAsync()
        };
    }

    public async Task<SurgeryListViewModel?> GetSurgeryListAsync(int admissionId)
    {
        var admission = await _db.Admissions
            .Include(a => a.Patient)
            .Include(a => a.Bed).ThenInclude(b => b!.Ward)
            .Include(a => a.AttendingDoctor)
            .FirstOrDefaultAsync(a => a.AdmissionId == admissionId);

        if (admission == null) return null;

        return new SurgeryListViewModel
        {
            Admission = admission,
            Surgeries = await _db.Surgeries
                .Include(s => s.Surgeon)
                .Include(s => s.Anesthesiologist)
                .Where(s => s.AdmissionId == admissionId)
                .OrderByDescending(s => s.ScheduledAt)
                .ToListAsync(),
            Doctors = await _db.Staffs
                .Where(s => s.StaffType == "DOCTOR")
                .OrderBy(s => s.FullName)
                .ToListAsync()
        };
    }

    private async Task<List<DashboardActivity>> BuildActivitiesAsync(DateTime todayStart)
    {
        var activities = new List<DashboardActivity>();

        var newAdmissions = await _db.Admissions
            .Include(a => a.Patient).Include(a => a.Bed).ThenInclude(b => b!.Ward)
            .Where(a => a.AdmittedAt >= todayStart.AddDays(-1))
            .OrderByDescending(a => a.AdmittedAt).Take(10).ToListAsync();

        foreach (var a in newAdmissions)
        {
            activities.Add(new DashboardActivity
            {
                At = a.AdmittedAt,
                Icon = "bi-hospital",
                Title = $"Nhập viện - {a.Patient?.FullName}",
                Detail = $"Buồng {a.Bed?.Ward?.Name} - Giường {a.Bed?.BedNumber}",
                Url = $"/Inpatient/Home/Detail/{a.AdmissionId}",
                Tag = "Nhập viện",
                Priority = "warning"
            });
        }

        var newOrders = await _db.MedicalOrders
            .Include(o => o.Admission)!.ThenInclude(a => a!.Patient)
            .Where(o => o.OrderedAt >= todayStart.AddDays(-1))
            .OrderByDescending(o => o.OrderedAt).Take(10).ToListAsync();

        foreach (var o in newOrders)
        {
            activities.Add(new DashboardActivity
            {
                At = o.OrderedAt,
                Icon = "bi-clipboard-pulse",
                Title = $"Lệnh điều trị: {o.OrderType}",
                Detail = $"BN {o.Admission?.Patient?.FullName} - {o.OrderDetails}",
                Url = $"/Inpatient/MedicalOrder/Index?admissionId={o.AdmissionId}",
                Tag = o.Status.ToString(),
                Priority = o.Status == MedicalOrderStatus.Ordered ? "warning" : "success"
            });
        }

        return activities.OrderByDescending(x => x.At).Take(20).ToList();
    }

    public async Task<WardListViewModel> GetWardListAsync()
    {
        var bedCounts = await _db.Beds
            .GroupBy(b => b.WardId)
            .Select(g => new { WardId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.WardId, x => x.Count);

        return new WardListViewModel
        {
            Wards = await _db.Wards
                .Include(w => w.Department)
                .OrderBy(w => w.Name)
                .ToListAsync(),
            BedCounts = bedCounts
        };
    }

    public async Task<WardFormViewModel?> GetWardFormAsync(int? id = null)
    {
        Ward ward;
        if (id.HasValue)
        {
            ward = await _db.Wards.FindAsync(id.Value) ?? null!;
            if (ward == null) return null;
        }
        else
        {
            ward = new Ward();
        }

        return new WardFormViewModel
        {
            Ward = ward,
            Departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync()
        };
    }

    public async Task<BedMapViewModel?> GetBedMapAsync(int wardId)
    {
        var ward = await _db.Wards
            .Include(w => w.Department)
            .FirstOrDefaultAsync(w => w.WardId == wardId);

        if (ward == null) return null;

        var beds = await _db.Beds
            .Where(b => b.WardId == wardId)
            .OrderBy(b => b.BedNumber)
            .ToListAsync();

        var occupiedBedIds = beds
            .Where(b => b.Status == BedStatus.Occupied)
            .Select(b => b.BedId)
            .ToList();

        var admissions = await _db.Admissions
            .Include(a => a.Patient)
            .Where(a => occupiedBedIds.Contains(a.BedId) && a.Status == AdmissionStatus.Active)
            .ToDictionaryAsync(a => a.BedId, a => a);

        return new BedMapViewModel
        {
            Ward = ward,
            Beds = beds,
            AdmissionsByBed = admissions
        };
    }

    public async Task<AddBedFormViewModel?> GetAddBedFormAsync(int wardId)
    {
        var ward = await _db.Wards.FindAsync(wardId);
        if (ward == null) return null;

        return new AddBedFormViewModel
        {
            Ward = ward,
            Bed = new Bed { WardId = wardId }
        };
    }
}
