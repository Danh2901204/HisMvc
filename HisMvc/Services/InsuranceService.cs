using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services;

public class InsuranceService
{
    private readonly AppDbContext _db;

    public InsuranceService(AppDbContext db)
    {
        _db = db;
    }

    // Tính toán chi phí BHYT cho l??t khám
    public async Task<InsuranceCalculationResult> CalculateInsuranceForEncounter(int encounterId)
    {
        var encounter = await _db.Encounters
            .Include(e => e.Patient)
            .FirstOrDefaultAsync(e => e.EncounterId == encounterId);

        if (encounter == null || encounter.Patient == null)
        {
            return new InsuranceCalculationResult { HasInsurance = false };
        }

        var patient = encounter.Patient;

        // Ki?m tra có BHYT không
        if (string.IsNullOrEmpty(patient.InsuranceNumber))
        {
            return new InsuranceCalculationResult { HasInsurance = false };
        }

        // Ki?m tra còn h?n không
        if (patient.InsuranceExpiry == null || patient.InsuranceExpiry < DateTime.Today)
        {
            return new InsuranceCalculationResult 
            { 
                HasInsurance = true, 
                IsExpired = true,
                ErrorMessage = "Th? BHYT ?ã h?t h?n!" 
            };
        }

        // L?y danh sách d?ch v?
        var orders = await _db.Orders
            .Include(o => o.Service)
            .Where(o => o.EncounterId == encounterId)
            .ToListAsync();

        // Tính chi phí
        decimal examFee = HisConstants.EXAM_FEE;
        decimal totalServiceCost = orders.Sum(o => o.Service?.Price ?? 0);
        decimal totalCost = examFee + totalServiceCost;

        // Tính BHYT chi tr?
        decimal coveragePercent = patient.InsuranceCoveragePercent;
        if (coveragePercent == 0)
        {
            coveragePercent = HisConstants.DEFAULT_INSURANCE_COVERAGE;
        }

        decimal insurancePays = totalCost * (coveragePercent / 100);
        decimal patientPays = totalCost - insurancePays;

        return new InsuranceCalculationResult
        {
            HasInsurance = true,
            IsValid = true,
            InsuranceNumber = patient.InsuranceNumber,
            InsuranceType = patient.InsuranceType ?? "KC",
            CoveragePercent = coveragePercent,
            TotalAmount = totalCost,
            InsurancePays = insurancePays,
            PatientPays = patientPays,
            ExamFee = examFee,
            ServiceFee = totalServiceCost
        };
    }

    // T?o giám ??nh BHYT
    public async Task<InsuranceClaim> CreateInsuranceClaim(int encounterId, InsuranceCalculationResult calculation)
    {
        var encounter = await _db.Encounters
            .Include(e => e.Patient)
            .FirstOrDefaultAsync(e => e.EncounterId == encounterId);

        if (encounter == null || !calculation.IsValid)
        {
            throw new InvalidOperationException("Không th? t?o giám ??nh BHYT");
        }

        var claim = new InsuranceClaim
        {
            ClaimCode = $"CLAIM{DateTime.UtcNow:yyyyMMddHHmmss}",
            EncounterId = encounterId,
            PatientId = encounter.PatientId,
            InsuranceNumber = calculation.InsuranceNumber,
            InsuranceExpiry = encounter.Patient!.InsuranceExpiry!.Value,
            InsuranceType = calculation.InsuranceType,
            CoveragePercent = calculation.CoveragePercent,
            TotalAmount = calculation.TotalAmount,
            InsuranceCovered = calculation.InsurancePays,
            PatientPayment = calculation.PatientPays,
            Status = ClaimStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.InsuranceClaims.Add(claim);
        await _db.SaveChangesAsync();

        return claim;
    }
}

public class InsuranceCalculationResult
{
    public bool HasInsurance { get; set; }
    public bool IsValid { get; set; }
    public bool IsExpired { get; set; }
    public string? ErrorMessage { get; set; }
    
    public string InsuranceNumber { get; set; } = "";
    public string InsuranceType { get; set; } = "";
    public decimal CoveragePercent { get; set; }
    
    public decimal TotalAmount { get; set; }
    public decimal InsurancePays { get; set; }
    public decimal PatientPays { get; set; }
    
    public decimal ExamFee { get; set; }
    public decimal ServiceFee { get; set; }
}
