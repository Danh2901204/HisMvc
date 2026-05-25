using System.ComponentModel.DataAnnotations;

namespace HisMvc.Entities;

public class Invoice
{
    public int InvoiceId { get; set; }

    // Mot trong hai phai co (encounter cho ngoai tru, admission cho noi tru)
    public int? EncounterId { get; set; }
    public Encounter? Encounter { get; set; }

    public int? AdmissionId { get; set; }
    public Admission? Admission { get; set; }

    [MaxLength(50)]
    public string InvoiceCode { get; set; } = "";

    // Loai hóa đơn - phan biet ExamFee / Services / Medicine / Final / Inpatient
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Final;

    public decimal TotalAmount { get; set; }

    // Phân tách tien theo nhom (chỉ có ý nghĩa voi Final/Inpatient)
    public decimal ExamFeeAmount { get; set; } = 0;
    public decimal ServicesAmount { get; set; } = 0;
    public decimal MedicineAmount { get; set; } = 0;
    public decimal BedAmount { get; set; } = 0; // Tien giường (noi tru)

    // BHYT Integration
    public decimal InsuranceAmount { get; set; } = 0; // So tien BHYT chi trả

    public decimal PatientAmount { get; set; } = 0; // So tien bệnh nhân phai tra

    public bool HasInsurance { get; set; } = false; // Co su dung BHYT không

    public int? InsuranceClaimId { get; set; } // Link den giam dinh BHYT
    public InsuranceClaim? InsuranceClaim { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    [MaxLength(200)]
    public string? PaidBy { get; set; }

    public int? PaidByStaffId { get; set; } // Thu ngan
    public Staff? PaidByStaff { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    // Hoa don dien tu
    [MaxLength(100)]
    public string? TaxCode { get; set; }

    [MaxLength(200)]
    public string? EInvoiceCode { get; set; }

    public DateTime? EInvoiceIssuedAt { get; set; }

    // Concurrency
    [Timestamp] public byte[]? RowVersion { get; set; }
}

public enum InvoiceStatus
{
    Unpaid = 1,
    Paid = 2,
    Cancelled = 9
}
