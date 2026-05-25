using HisMvc.Areas.Admin.Models;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Admin.Services;

public class AdminViewService
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public AdminViewService(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<AdminDashboardViewModel> BuildDashboardAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayStart = DateTime.Today;

        var vm = new AdminDashboardViewModel
        {
            Kpi = new AdminKpiViewModel
            {
                TotalDepartments = await _db.Departments.CountAsync(),
                TotalStaff = await _db.Staffs.CountAsync(),
                TotalServices = await _db.Services.CountAsync(),
                TotalPatients = await _db.Patients.CountAsync(),
                TotalAppointmentsToday = await _db.Appointments.CountAsync(x => x.Date == today),
                TotalEncountersToday = await _db.Encounters.CountAsync(x => x.CheckInAt.Date == DateTime.Today),
                ActiveAdmissions = await _db.Admissions.CountAsync(a => a.Status == AdmissionStatus.Active),
                UnpaidInvoices = await _db.Invoices.CountAsync(i => i.Status == InvoiceStatus.Unpaid),
                PendingClaims = await _db.InsuranceClaims.CountAsync(c =>
                    c.Status == ClaimStatus.Pending || c.Status == ClaimStatus.Submitted),
                PendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Requested),
                PendingPrescriptions = await _db.Prescriptions.CountAsync(p => p.Status == PrescriptionStatus.Pending),
                RevenueToday = await _db.Invoices
                    .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt.HasValue && i.PaidAt.Value.Date == todayStart)
                    .SumAsync(i => i.PatientAmount)
            }
        };

        vm.Activities = await BuildActivitiesAsync();
        return vm;
    }

    public async Task<PatientListViewModel> GetPatientListAsync(string search)
    {
        var query = _db.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.FullName.Contains(term) ||
                x.Phone.Contains(term) ||
                (x.IdentityNumber != null && x.IdentityNumber.Contains(term)) ||
                (x.InsuranceNumber != null && x.InsuranceNumber.Contains(term)));
        }

        return new PatientListViewModel
        {
            Patients = await query.OrderBy(x => x.PatientId).Take(200).ToListAsync(),
            Search = search
        };
    }

    public async Task<PatientDetailViewModel?> GetPatientDetailAsync(int id)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient == null) return null;

        return new PatientDetailViewModel
        {
            Patient = patient,
            Allergies = await _db.Allergies
                .Where(x => x.PatientId == id)
                .OrderByDescending(x => x.IdentifiedDate)
                .ToListAsync(),
            MedicalHistories = await _db.MedicalHistories
                .Where(x => x.PatientId == id)
                .OrderByDescending(x => x.DiagnosedDate)
                .ToListAsync()
        };
    }

    public async Task<InsuranceClaimListViewModel> GetInsuranceClaimListAsync(ClaimStatus? status, string? search)
    {
        var query = _db.InsuranceClaims
            .Include(c => c.Patient)
            .Include(c => c.Encounter)!.ThenInclude(e => e!.Doctor)
            .Include(c => c.ApprovedByStaff)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.Trim();
            query = query.Where(c => c.ClaimCode.Contains(t)
                || c.InsuranceNumber.Contains(t)
                || (c.Patient != null && c.Patient.FullName.Contains(t)));
        }

        return new InsuranceClaimListViewModel
        {
            Claims = await query.OrderBy(c => c.InsuranceClaimId).Take(200).ToListAsync(),
            SelectedStatus = status,
            Search = search ?? "",
            Stats = new InsuranceClaimStatsViewModel
            {
                Pending = await _db.InsuranceClaims.CountAsync(c => c.Status == ClaimStatus.Pending),
                Submitted = await _db.InsuranceClaims.CountAsync(c => c.Status == ClaimStatus.Submitted),
                Approved = await _db.InsuranceClaims.CountAsync(c => c.Status == ClaimStatus.Approved),
                Rejected = await _db.InsuranceClaims.CountAsync(c => c.Status == ClaimStatus.Rejected),
                TotalInsurancePaid = await _db.InsuranceClaims
                    .Where(c => c.Status == ClaimStatus.Approved)
                    .SumAsync(c => c.InsuranceCovered)
            }
        };
    }

    public async Task<InsuranceClaimDetailViewModel?> GetInsuranceClaimDetailAsync(int id)
    {
        var claim = await _db.InsuranceClaims
            .Include(c => c.Patient)
            .Include(c => c.Encounter)!.ThenInclude(e => e!.Doctor)
            .Include(c => c.Admission)
            .Include(c => c.ApprovedByStaff)
            .FirstOrDefaultAsync(c => c.InsuranceClaimId == id);

        if (claim == null) return null;

        return new InsuranceClaimDetailViewModel
        {
            Claim = claim,
            Items = await _db.InsuranceClaimItems
                .Where(i => i.InsuranceClaimId == id)
                .OrderBy(i => i.InsuranceClaimItemId)
                .ToListAsync()
        };
    }

    private async Task<List<DashboardActivity>> BuildActivitiesAsync()
    {
        var activities = new List<DashboardActivity>();

        var newAppointments = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.TimeSlot)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .ToListAsync();

        foreach (var a in newAppointments)
        {
            activities.Add(new DashboardActivity
            {
                At = a.CreatedAt,
                Icon = "bi-calendar-plus",
                Title = $"Lịch hẹn moi - {a.Patient?.FullName}",
                Detail = $"BS {a.Doctor?.FullName} · {a.Date:dd/MM} {a.TimeSlot?.Start:HH:mm}",
                Url = "/Reception/Home",
                Tag = "Le Tan"
            });
        }

        var newEncounters = await _db.Encounters
            .Include(e => e.Patient).Include(e => e.Doctor)
            .OrderByDescending(e => e.CheckInAt).Take(5).ToListAsync();

        foreach (var e in newEncounters)
        {
            activities.Add(new DashboardActivity
            {
                At = e.CheckInAt,
                Icon = "bi-stethoscope",
                Title = $"Kham - {e.Patient?.FullName}",
                Detail = $"Tinh trang: {e.Status} · BS {e.Doctor?.FullName}",
                Url = $"/Doctor/Home/Examine/{e.EncounterId}",
                Tag = "Bác sĩ",
                Priority = e.Status == EncounterStatus.CheckedIn ? "warning" : ""
            });
        }

        var newOrders = await _db.Orders
            .Include(o => o.Service)
            .Include(o => o.Encounter)!.ThenInclude(e => e!.Patient)
            .Where(o => o.Status == OrderStatus.Requested)
            .OrderByDescending(o => o.OrderedAt).Take(5).ToListAsync();

        foreach (var o in newOrders)
        {
            activities.Add(new DashboardActivity
            {
                At = o.OrderedAt,
                Icon = "bi-flask",
                Title = $"Chỉ định {o.Service?.Type} - {o.Encounter?.Patient?.FullName}",
                Detail = o.Service?.Name ?? "",
                Url = "/Lab/Home",
                Tag = "Lab"
            });
        }

        var paidInvoices = await _db.Invoices
            .Include(i => i.Encounter)!.ThenInclude(e => e!.Patient)
            .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt.HasValue)
            .OrderByDescending(i => i.PaidAt!.Value).Take(5).ToListAsync();

        foreach (var i in paidInvoices)
        {
            activities.Add(new DashboardActivity
            {
                At = i.PaidAt!.Value,
                Icon = "bi-cash-coin",
                Title = $"Thu tien {i.InvoiceCode}",
                Detail = $"{i.PatientAmount:N0} VND · {i.Encounter?.Patient?.FullName}",
                Url = $"/Cashier/Home/Detail/{i.InvoiceId}",
                Tag = "Thu ngân",
                Priority = "success"
            });
        }

        var admissions = await _db.Admissions
            .Include(a => a.Patient).Include(a => a.Bed).ThenInclude(b => b!.Ward)
            .Where(a => a.Status == AdmissionStatus.Active)
            .OrderByDescending(a => a.AdmittedAt).Take(5).ToListAsync();

        foreach (var ad in admissions)
        {
            activities.Add(new DashboardActivity
            {
                At = ad.AdmittedAt,
                Icon = "bi-hospital",
                Title = $"Nhập viện - {ad.Patient?.FullName}",
                Detail = $"Buồng {ad.Bed?.Ward?.Name}, giường {ad.Bed?.BedNumber}",
                Url = $"/Inpatient/Home/Detail/{ad.AdmissionId}",
                Tag = "Nội trú"
            });
        }

        return activities.OrderByDescending(x => x.At).Take(20).ToList();
    }

    public async Task<DepartmentListViewModel> GetDepartmentListAsync() => new()
    {
        Departments = await _db.Departments.OrderBy(x => x.DepartmentId).ToListAsync()
    };

    public async Task<ServiceListViewModel> GetServiceListAsync() => new()
    {
        Services = await _db.Services.OrderBy(x => x.ServiceId).ToListAsync()
    };

    public async Task<TimeSlotListViewModel> GetTimeSlotListAsync() => new()
    {
        TimeSlots = await _db.TimeSlots.OrderBy(x => x.TimeSlotId).ToListAsync()
    };

    public async Task<InsuranceConfigListViewModel> GetInsuranceConfigListAsync() => new()
    {
        Configs = await _db.InsuranceConfigs.OrderBy(x => x.InsuranceConfigId).ToListAsync()
    };

    public async Task<StaffListViewModel> GetStaffListAsync() => new()
    {
        Staffs = await _db.Staffs.Include(x => x.Department).OrderBy(x => x.StaffId).ToListAsync()
    };

    public async Task<StaffFormViewModel?> GetStaffFormAsync(int? id = null)
    {
        Staff staff;
        if (id.HasValue)
        {
            staff = await _db.Staffs.FindAsync(id.Value) ?? null!;
            if (staff == null) return null;
        }
        else
        {
            staff = new Staff();
        }

        var departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
        return new StaffFormViewModel
        {
            Staff = staff,
            Departments = new SelectList(departments, "DepartmentId", "Name", staff.DepartmentId)
        };
    }

    public async Task<UserListViewModel> GetUserListAsync()
    {
        var vm = new UserListViewModel();
        var users = await _userManager.Users
            .OrderBy(u => u.StaffId ?? int.MaxValue)
            .ThenBy(u => u.UserName)
            .ToListAsync();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            vm.Users.Add(new UserAccountRowViewModel
            {
                User = user,
                Roles = string.Join(", ", roles)
            });
        }
        return vm;
    }

    public async Task<UserFormViewModel> GetUserCreateFormAsync()
    {
        var staffs = await _db.Staffs.OrderBy(s => s.FullName).ToListAsync();
        var roles = await _db.Roles.OrderBy(r => r.Name).Select(r => r.Name!).ToListAsync();
        return new UserFormViewModel
        {
            Roles = roles,
            Staffs = new SelectList(staffs, "StaffId", "FullName")
        };
    }

    public async Task<UserFormViewModel?> GetUserEditFormAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var userRoles = await _userManager.GetRolesAsync(user);
        var staffs = await _db.Staffs.OrderBy(s => s.FullName).ToListAsync();
        var roles = await _db.Roles.OrderBy(r => r.Name).Select(r => r.Name!).ToListAsync();

        return new UserFormViewModel
        {
            User = user,
            CurrentRole = userRoles.FirstOrDefault(),
            Roles = roles,
            Staffs = new SelectList(staffs, "StaffId", "FullName", user.StaffId)
        };
    }
}
