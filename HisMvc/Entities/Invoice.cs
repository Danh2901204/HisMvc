using System.ComponentModel.DataAnnotations;

namespace HisMvc.Entities;

public class Invoice
{
    public int InvoiceId { get; set; }

    public int EncounterId { get; set; }
    public Encounter? Encounter { get; set; }

    [MaxLength(50)]
    public string InvoiceCode { get; set; } = "";

    public decimal TotalAmount { get; set; }
    
    // BHYT Integration
    public decimal InsuranceAmount { get; set; } = 0; // S? ti?n BHYT chi tr?
    
    public decimal PatientAmount { get; set; } = 0; // S? ti?n b?nh nhân ph?i tr?
    
    public bool HasInsurance { get; set; } = false; // Có s? d?ng BHYT không
    
    public int? InsuranceClaimId { get; set; } // Link ??n giám ??nh BHYT
    public InsuranceClaim? InsuranceClaim { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    [MaxLength(200)]
    public string? PaidBy { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
    
    // Hóa ??n ?i?n t?
    [MaxLength(100)]
    public string? TaxCode { get; set; } // Mã s? thu? (n?u có)
    
    [MaxLength(200)]
    public string? EInvoiceCode { get; set; } // Mã hóa ??n ?i?n t?
    
    public DateTime? EInvoiceIssuedAt { get; set; } // Ngày xu?t hóa ??n ?i?n t?
}

public enum InvoiceStatus
{
    Unpaid = 1,
    Paid = 2
}
