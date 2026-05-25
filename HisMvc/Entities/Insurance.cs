using System.ComponentModel.DataAnnotations;

namespace HisMvc.Entities;

// 1. Giám ??nh BHYT cho l??t khám/nh?p vi?n
public class InsuranceClaim
{
    public int InsuranceClaimId { get; set; }
    
    [MaxLength(30)]
    public string ClaimCode { get; set; } = "";
    
    // Link ??n Encounter ho?c Admission
    public int? EncounterId { get; set; }
    public Encounter? Encounter { get; set; }
    
    public int? AdmissionId { get; set; }
    public Admission? Admission { get; set; }
    
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    
    // Thông tin th? BHYT t?i th?i ?i?m khám
    [MaxLength(15)]
    public string InsuranceNumber { get; set; } = "";
    
    public DateTime InsuranceExpiry { get; set; }
    
    [MaxLength(10)]
    public string InsuranceType { get; set; } = "";
    
    public decimal CoveragePercent { get; set; } = 80; // % chi tr?
    
    // Chi phí
    public decimal TotalAmount { get; set; } // T?ng chi phí
    
    public decimal InsuranceCovered { get; set; } // BHYT chi tr?
    
    public decimal PatientPayment { get; set; } // B?nh nhân ??ng chi tr?
    
    // Tr?ng thái giám ??nh
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? SubmittedAt { get; set; } // Ngày g?i giám ??nh
    
    public DateTime? ApprovedAt { get; set; } // Ngày duy?t
    
    public int? ApprovedBy { get; set; }
    public Staff? ApprovedByStaff { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    [MaxLength(500)]
    public string? RejectReason { get; set; }
    
    // XML data theo chu?n B? Y t?
    public string? XmlData { get; set; } // L?u XML ?? g?i lên c?ng BHYT
}

// 2. Chi ti?t chi phí BHYT (theo d?ch v?)
public class InsuranceClaimItem
{
    public int InsuranceClaimItemId { get; set; }
    
    public int InsuranceClaimId { get; set; }
    public InsuranceClaim? InsuranceClaim { get; set; }
    
    [MaxLength(200)]
    public string ServiceName { get; set; } = "";
    
    [MaxLength(50)]
    public string ServiceCode { get; set; } = ""; // Mã d?ch v? theo BHYT
    
    public int Quantity { get; set; } = 1;
    
    public decimal UnitPrice { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public decimal InsurancePaid { get; set; } // BHYT tr?
    
    public decimal PatientPaid { get; set; } // BN tr?
    
    public bool IsInInsuranceList { get; set; } = true; // Có trong danh m?c BHYT không
    
    [MaxLength(500)]
    public string? Note { get; set; }
}

// 3. C?u hình t? l? BHYT theo lo?i th?
public class InsuranceConfig
{
    public int InsuranceConfigId { get; set; }
    
    [MaxLength(10)]
    public string InsuranceType { get; set; } = ""; // QN, KC, TE, CB...
    
    [MaxLength(200)]
    public string Description { get; set; } = "";
    
    public decimal DefaultCoveragePercent { get; set; } = 80;
    
    public bool RequireRegistration { get; set; } = true; // Yêu c?u ??ng ký n?i KCB
    
    public bool IsActive { get; set; } = true;
}
