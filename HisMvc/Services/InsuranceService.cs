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

    // Tính toán chi phí BHYT cho lượt khám
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

        // Kiểm tra có BHYT không
        if (string.IsNullOrEmpty(patient.InsuranceNumber))
        {
            return new InsuranceCalculationResult { HasInsurance = false };
        }

        // Kiểm tra còn hạn không
        if (patient.InsuranceExpiry == null || patient.InsuranceExpiry < DateTime.Today)
        {
            return new InsuranceCalculationResult 
            { 
                HasInsurance = true, 
                IsExpired = true,
                ErrorMessage = "Thẻ BHYT đã hết hạn!" 
            };
        }

        // Lấy danh sách dịch vụ (loại bỏ Cancelled)
        var orders = await _db.Orders
            .Include(o => o.Service)
            .Where(o => o.EncounterId == encounterId && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        decimal examFee = HisConstants.EXAM_FEE;
        decimal totalServiceCost = orders.Sum(o => (o.Service?.Price ?? 0) * Math.Max(1, o.Quantity));

        // Tiền thuốc (theo đơn thuốc chưa hủy)
        decimal medicineCost = 0;
        var rx = await _db.Prescriptions
            .Include(p => p.Items)!.ThenInclude(it => it.Medicine)
            .FirstOrDefaultAsync(p => p.EncounterId == encounterId && p.Status != PrescriptionStatus.Cancelled);
        if (rx != null)
        {
            foreach (var it in rx.Items)
            {
                decimal unitPrice = it.Medicine?.BhytPrice ?? 0;
                if (unitPrice <= 0)
                {
                    unitPrice = await _db.MedicineBatches
                        .Where(b => b.MedicineId == it.MedicineId && b.IsActive)
                        .OrderByDescending(b => b.MedicineBatchId)
                        .Select(b => b.UnitPrice)
                        .FirstOrDefaultAsync();
                }
                medicineCost += unitPrice * it.Quantity;
            }
        }

        decimal totalCost = examFee + totalServiceCost + medicineCost;

        // Tính BHYT chi trả
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
            ServiceFee = totalServiceCost,
            MedicineFee = medicineCost
        };
    }

    // Tạo giám định BHYT - sinh ca InsuranceClaim va Items
    // Theo QD 4210/QD-BYT 2017 va 130/QD-BHXH ve chuan du lieu XML giam dinh
    public async Task<InsuranceClaim> CreateInsuranceClaim(int encounterId, InsuranceCalculationResult calculation)
    {
        var encounter = await _db.Encounters
            .Include(e => e.Patient)
            .FirstOrDefaultAsync(e => e.EncounterId == encounterId);

        if (encounter == null || !calculation.IsValid)
        {
            throw new InvalidOperationException("Không thể tạo giám định BHYT");
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

        // Sinh InsuranceClaimItems tu cac dịch vụ da chi dinh + phi kham
        await GenerateClaimItemsAsync(claim, encounterId, calculation.CoveragePercent);
        await _db.SaveChangesAsync();

        return claim;
    }

    // Sinh chi tiết chi phí BHYT (theo 130/QD-BHXH - XML 2)
    private async Task GenerateClaimItemsAsync(InsuranceClaim claim, int encounterId, decimal coveragePercent)
    {
        // Phí khám
        _db.InsuranceClaimItems.Add(new InsuranceClaimItem
        {
            InsuranceClaimId = claim.InsuranceClaimId,
            ServiceName = "Phí khám bệnh",
            ServiceCode = "PK01",
            Quantity = 1,
            UnitPrice = HisConstants.EXAM_FEE,
            TotalPrice = HisConstants.EXAM_FEE,
            InsurancePaid = Math.Round(HisConstants.EXAM_FEE * coveragePercent / 100m, 0),
            PatientPaid = Math.Round(HisConstants.EXAM_FEE * (100m - coveragePercent) / 100m, 0),
            IsInInsuranceList = true
        });

        // Cac dịch vụ
        var orders = await _db.Orders
            .Include(o => o.Service)
            .Where(o => o.EncounterId == encounterId && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        foreach (var o in orders)
        {
            if (o.Service == null) continue;
            var qty = Math.Max(1, o.Quantity);
            var unit = o.Service.BhytPrice ?? o.Service.Price;
            var total = unit * qty;
            var insPaid = Math.Round(total * coveragePercent / 100m, 0);
            _db.InsuranceClaimItems.Add(new InsuranceClaimItem
            {
                InsuranceClaimId = claim.InsuranceClaimId,
                ServiceName = o.Service.Name,
                ServiceCode = o.Service.BhytCode ?? o.Service.Code,
                Quantity = qty,
                UnitPrice = unit,
                TotalPrice = total,
                InsurancePaid = insPaid,
                PatientPaid = total - insPaid,
                IsInInsuranceList = o.Service.IsInBhytList
            });
        }

        // Tiền thuốc
        var rx = await _db.Prescriptions
            .Include(p => p.Items)!.ThenInclude(it => it.Medicine)
            .FirstOrDefaultAsync(p => p.EncounterId == encounterId && p.Status != PrescriptionStatus.Cancelled);
        if (rx != null)
        {
            foreach (var it in rx.Items)
            {
                if (it.Medicine == null) continue;
                decimal unitPrice = it.Medicine.BhytPrice ?? 0;
                if (unitPrice <= 0)
                {
                    unitPrice = await _db.MedicineBatches
                        .Where(b => b.MedicineId == it.MedicineId && b.IsActive)
                        .OrderByDescending(b => b.MedicineBatchId)
                        .Select(b => b.UnitPrice)
                        .FirstOrDefaultAsync();
                }
                var total = unitPrice * it.Quantity;
                var insPaid = Math.Round(total * coveragePercent / 100m, 0);
                _db.InsuranceClaimItems.Add(new InsuranceClaimItem
                {
                    InsuranceClaimId = claim.InsuranceClaimId,
                    ServiceName = it.Medicine.Name,
                    ServiceCode = it.Medicine.BhytCode ?? it.Medicine.Code,
                    Quantity = it.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = total,
                    InsurancePaid = insPaid,
                    PatientPaid = total - insPaid,
                    IsInInsuranceList = it.Medicine.IsInBhytList
                });
            }
        }
    }

    // Tạo XML giam dinh chuan 130/QD-BHXH (XML 1 - thông tin chung)
    public async Task<string> ExportClaimXmlAsync(int claimId)
    {
        var claim = await _db.InsuranceClaims
            .Include(c => c.Patient)
            .Include(c => c.Encounter)
            .FirstOrDefaultAsync(c => c.InsuranceClaimId == claimId);

        if (claim == null) return string.Empty;

        var items = await _db.InsuranceClaimItems
            .Where(i => i.InsuranceClaimId == claimId)
            .ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<HOSO>");
        sb.AppendLine("  <FILEHOSO>");
        sb.AppendLine("    <LOAIHOSO>XML1</LOAIHOSO>");
        sb.AppendLine("    <NOIDUNGFILE>");
        sb.AppendLine("      <TONG_HOP>");
        sb.AppendLine($"        <MA_LK>{System.Security.SecurityElement.Escape(claim.ClaimCode)}</MA_LK>");
        sb.AppendLine($"        <STT>1</STT>");
        sb.AppendLine($"        <MA_BN>BN{claim.PatientId}</MA_BN>");
        sb.AppendLine($"        <HO_TEN>{System.Security.SecurityElement.Escape(claim.Patient?.FullName ?? "")}</HO_TEN>");
        sb.AppendLine($"        <SO_THE_BHYT>{System.Security.SecurityElement.Escape(claim.InsuranceNumber)}</SO_THE_BHYT>");
        sb.AppendLine($"        <NGAY_HET_HAN>{claim.InsuranceExpiry:yyyyMMdd}</NGAY_HET_HAN>");
        sb.AppendLine($"        <TEN_BENH>{System.Security.SecurityElement.Escape(claim.Encounter?.Diagnosis ?? "")}</TEN_BENH>");
        sb.AppendLine($"        <T_TONGCHI>{claim.TotalAmount}</T_TONGCHI>");
        sb.AppendLine($"        <T_BHTT>{claim.InsuranceCovered}</T_BHTT>");
        sb.AppendLine($"        <T_BNTT>{claim.PatientPayment}</T_BNTT>");
        sb.AppendLine($"        <NGAY_LAP>{claim.CreatedAt:yyyyMMddHHmm}</NGAY_LAP>");
        sb.AppendLine("      </TONG_HOP>");
        sb.AppendLine("    </NOIDUNGFILE>");
        sb.AppendLine("  </FILEHOSO>");
        sb.AppendLine("  <FILEHOSO>");
        sb.AppendLine("    <LOAIHOSO>XML2</LOAIHOSO>");
        sb.AppendLine("    <NOIDUNGFILE>");
        foreach (var it in items)
        {
            sb.AppendLine("      <CHI_TIET_DVKT>");
            sb.AppendLine($"        <MA_LK>{System.Security.SecurityElement.Escape(claim.ClaimCode)}</MA_LK>");
            sb.AppendLine($"        <MA_DICH_VU>{System.Security.SecurityElement.Escape(it.ServiceCode)}</MA_DICH_VU>");
            sb.AppendLine($"        <TEN_DICH_VU>{System.Security.SecurityElement.Escape(it.ServiceName)}</TEN_DICH_VU>");
            sb.AppendLine($"        <SO_LUONG>{it.Quantity}</SO_LUONG>");
            sb.AppendLine($"        <DON_GIA>{it.UnitPrice}</DON_GIA>");
            sb.AppendLine($"        <THANH_TIEN>{it.TotalPrice}</THANH_TIEN>");
            sb.AppendLine($"        <T_BHTT>{it.InsurancePaid}</T_BHTT>");
            sb.AppendLine($"        <T_BNTT>{it.PatientPaid}</T_BNTT>");
            sb.AppendLine("      </CHI_TIET_DVKT>");
        }
        sb.AppendLine("    </NOIDUNGFILE>");
        sb.AppendLine("  </FILEHOSO>");
        sb.AppendLine("</HOSO>");

        // Lưu vào DB
        claim.XmlData = sb.ToString();
        await _db.SaveChangesAsync();
        return sb.ToString();
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
    public decimal MedicineFee { get; set; }
}
