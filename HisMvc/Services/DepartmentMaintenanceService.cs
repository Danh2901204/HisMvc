using HisMvc.Data;
using HisMvc.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HisMvc.Services;

/// <summary>Gộp khoa trùng, đồng bộ Kind lâm sàng, bổ sung bác sĩ mẫu.</summary>
public class DepartmentMaintenanceService
{
    private readonly AppDbContext _db;
    private readonly ILogger<DepartmentMaintenanceService> _logger;

    public DepartmentMaintenanceService(AppDbContext db, ILogger<DepartmentMaintenanceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SyncForPublicBookingAsync(CancellationToken ct = default)
    {
        await MergeDuplicateDepartmentsAsync(ct);
        await EnsureCatalogDepartmentsAsync(ct);
        await SyncClinicalKindsAsync(ct);
        await EnsureDemoDoctorsForClinicalDeptsAsync(ct);
    }

    private async Task MergeDuplicateDepartmentsAsync(CancellationToken ct)
    {
        var departments = await _db.Departments.OrderBy(d => d.DepartmentId).ToListAsync(ct);
        var canonicalByCode = new Dictionary<string, Department>(StringComparer.OrdinalIgnoreCase);
        var canonicalByNameKey = new Dictionary<string, Department>();

        foreach (var dept in departments)
        {
            if (ClinicalDepartmentCatalog.IsOutpatientClinical(dept.Code))
            {
                if (!canonicalByCode.TryGetValue(dept.Code, out var existing) ||
                    PreferCanonical(dept, existing))
                {
                    canonicalByCode[dept.Code] = dept;
                }
            }

            var nameKey = ClinicalDepartmentCatalog.NormalizeNameKey(dept.Name);
            if (!canonicalByNameKey.TryGetValue(nameKey, out var byName) ||
                PreferCanonical(dept, byName))
            {
                canonicalByNameKey[nameKey] = dept;
            }
        }

        foreach (var dept in departments.ToList())
        {
            Department? keeper = null;

            if (dept.Code.Equals("DEFAULT", StringComparison.OrdinalIgnoreCase))
            {
                keeper = canonicalByCode.GetValueOrDefault("KB")
                         ?? canonicalByNameKey.GetValueOrDefault("khámbệnh")
                         ?? canonicalByNameKey.GetValueOrDefault("khambenh");
            }
            else if (ClinicalDepartmentCatalog.IsOutpatientClinical(dept.Code))
            {
                canonicalByCode.TryGetValue(dept.Code, out keeper);
            }
            else
            {
                var key = ClinicalDepartmentCatalog.NormalizeNameKey(dept.Name);
                if (canonicalByNameKey.TryGetValue(key, out var byName) &&
                    ClinicalDepartmentCatalog.OutpatientClinical.Any(c =>
                        c.Code.Equals(byName.Code, StringComparison.OrdinalIgnoreCase) ||
                        ClinicalDepartmentCatalog.NormalizeNameKey(c.Name) == key))
                {
                    keeper = byName;
                }
            }

            if (keeper == null || keeper.DepartmentId == dept.DepartmentId)
                continue;

            await ReassignDepartmentReferencesAsync(dept.DepartmentId, keeper.DepartmentId, ct);
            _db.Departments.Remove(dept);
            _logger.LogInformation("Gộp khoa trùng {RemovedId}/{Code} → {KeeperId}/{KeeperName}",
                dept.DepartmentId, dept.Code, keeper.DepartmentId, keeper.Name);
        }

        await _db.SaveChangesAsync(ct);
    }

    private static bool PreferCanonical(Department candidate, Department current)
    {
        if (candidate.Name.StartsWith("Khoa ", StringComparison.OrdinalIgnoreCase) &&
            !current.Name.StartsWith("Khoa ", StringComparison.OrdinalIgnoreCase))
            return true;

        if (candidate.DepartmentId < current.DepartmentId &&
            candidate.Name.StartsWith("Khoa ", StringComparison.OrdinalIgnoreCase) ==
            current.Name.StartsWith("Khoa ", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private async Task ReassignDepartmentReferencesAsync(int fromId, int toId, CancellationToken ct)
    {
        await _db.Staffs.Where(s => s.DepartmentId == fromId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.DepartmentId, toId), ct);
        await _db.Appointments.Where(a => a.DepartmentId == fromId)
            .ExecuteUpdateAsync(a => a.SetProperty(x => x.DepartmentId, toId), ct);
        await _db.Encounters.Where(e => e.DepartmentId == fromId)
            .ExecuteUpdateAsync(e => e.SetProperty(x => x.DepartmentId, toId), ct);
        await _db.Wards.Where(w => w.DepartmentId == fromId)
            .ExecuteUpdateAsync(w => w.SetProperty(x => x.DepartmentId, toId), ct);
    }

    private async Task EnsureCatalogDepartmentsAsync(CancellationToken ct)
    {
        foreach (var (code, name) in ClinicalDepartmentCatalog.OutpatientClinical)
        {
            var existing = await _db.Departments.FirstOrDefaultAsync(d => d.Code == code, ct);
            if (existing == null)
            {
                _db.Departments.Add(new Department
                {
                    Code = code,
                    Name = name,
                    Kind = DepartmentKind.Clinical
                });
                continue;
            }

            existing.Name = name;
            existing.Kind = DepartmentKind.Clinical;
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task SyncClinicalKindsAsync(CancellationToken ct)
    {
        var all = await _db.Departments.ToListAsync(ct);
        foreach (var dept in all)
        {
            if (ClinicalDepartmentCatalog.IsOutpatientClinical(dept.Code))
                dept.Kind = DepartmentKind.Clinical;
            else if (dept.Code is "XN" or "CDHA" or "HS" or "LAB")
                dept.Kind = DepartmentKind.Paraclinical;
            else if (dept.Code is "HSCC" or "TTHS" or "CC")
                dept.Kind = DepartmentKind.InpatientOnly;
            else if (dept.Name.Contains("Công nghệ thông tin", StringComparison.OrdinalIgnoreCase) ||
                     dept.Name.Contains("Hành chính", StringComparison.OrdinalIgnoreCase) ||
                     dept.Name.Contains("Tài chính", StringComparison.OrdinalIgnoreCase) ||
                     dept.Code is "CNTT" or "HANHCHINH" or "TCKT" or "DUOC")
                dept.Kind = DepartmentKind.Administrative;
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureDemoDoctorsForClinicalDeptsAsync(CancellationToken ct)
    {
        var clinicalIds = await DepartmentBookingRules
            .BookableForPublic(_db.Departments)
            .Select(d => d.DepartmentId)
            .ToListAsync(ct);

        foreach (var deptId in clinicalIds)
        {
            var hasDoctor = await _db.Staffs.AnyAsync(
                s => s.DepartmentId == deptId && s.StaffType == "DOCTOR" && s.IsActive, ct);
            if (hasDoctor) continue;

            var deptName = await _db.Departments.Where(d => d.DepartmentId == deptId)
                .Select(d => d.Name).FirstAsync(ct);

            _db.Staffs.Add(new Staff
            {
                FullName = $"BS. {deptName.Replace("Khoa ", "")}",
                DepartmentId = deptId,
                StaffType = "DOCTOR",
                IsActive = true
            });
        }

        await _db.SaveChangesAsync(ct);
    }
}
