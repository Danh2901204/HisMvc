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

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    [MaxLength(200)]
    public string? PaidBy { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}

public enum InvoiceStatus
{
    Unpaid = 1,
    Paid = 2
}
