using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HisMvc.Entities;

// 1. Giám định BHYT cho lượt khám/nhập viện
public class InsuranceClaim
{
    public int InsuranceClaimId { get; set; }
    
    [MaxLength(30)]
    public string ClaimCode { get; set; } = "";
    
    // Link den Encounter hoặc Admission
    public int? EncounterId { get; set; }
    public Encounter? Encounter { get; set; }
    
    public int? AdmissionId { get; set; }
    public Admission? Admission { get; set; }
    
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    
    // Thông tin the BHYT tai thoi diem kham
    [MaxLength(15)]
    public string InsuranceNumber { get; set; } = "";
    
    public DateTime InsuranceExpiry { get; set; }
    
    [MaxLength(10)]
    public string InsuranceType { get; set; } = "";
    
    public decimal CoveragePercent { get; set; } = 80; // % chi tra
    
    // Chi phí
    public decimal TotalAmount { get; set; } // Tổng chi phí
    
    public decimal InsuranceCovered { get; set; } // BHYT chi trả
    
    public decimal PatientPayment { get; set; } // Bệnh nhân đồng chi trả
    
    // Trạng thái giam dinh
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? SubmittedAt { get; set; } // Ngay gui giam dinh
    
    public DateTime? ApprovedAt { get; set; } // Ngay duyet
    
    public int? ApprovedBy { get; set; }

    [ForeignKey(nameof(ApprovedBy))]
    public Staff? ApprovedByStaff { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    [MaxLength(500)]
    public string? RejectReason { get; set; }
    
    // XML data theo chuan Bo Y te
    public string? XmlData { get; set; } // Luu XML de gui len cong BHYT
}

// 2. Chi tiet chi phí BHYT (theo dịch vụ)
public class InsuranceClaimItem
{
    public int InsuranceClaimItemId { get; set; }
    
    public int InsuranceClaimId { get; set; }
    public InsuranceClaim? InsuranceClaim { get; set; }
    
    [MaxLength(200)]
    public string ServiceName { get; set; } = "";
    
    [MaxLength(50)]
    public string ServiceCode { get; set; } = ""; // Mã dịch vụ theo BHYT
    
    public int Quantity { get; set; } = 1;
    
    public decimal UnitPrice { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public decimal InsurancePaid { get; set; } // BHYT tra
    
    public decimal PatientPaid { get; set; } // BN tra
    
    public bool IsInInsuranceList { get; set; } = true; // Co trong danh muc BHYT không
    
    [MaxLength(500)]
    public string? Note { get; set; }
}

// 3. Cấu hình ty le BHYT theo loại thẻ
public class InsuranceConfig
{
    public int InsuranceConfigId { get; set; }
    
    [MaxLength(10)]
    public string InsuranceType { get; set; } = ""; // QN, KC, TE, CB...
    
    [MaxLength(200)]
    public string Description { get; set; } = "";
    
    public decimal DefaultCoveragePercent { get; set; } = 80;
    
    public bool RequireRegistration { get; set; } = true; // Yêu cầu dang ky noi KCB
    
    public bool IsActive { get; set; } = true;
}
