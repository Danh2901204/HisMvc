using System.ComponentModel.DataAnnotations;

namespace HisMvc.Entities;

public class Service
{
    public int ServiceId { get; set; }
    [MaxLength(50)] public string Code { get; set; } = "";  // Mã noi bo
    [MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(50)] public string Type { get; set; } = "LAB"; // LAB/IMAGING/EXAM/PROCEDURE
    public decimal Price { get; set; }

    // BHYT theo TT 22/2023 + QD 130/BHXH
    [MaxLength(20)] public string? BhytCode { get; set; }       // Mã BHYT
    public decimal? BhytPrice { get; set; }                     // Gia BHYT (neu khac)
    public bool IsInBhytList { get; set; } = true;
    [MaxLength(20)] public string? DepartmentCode { get; set; } // Mã khoa thuc hien

    public bool IsActive { get; set; } = true;
}

public class Order
{
    public int OrderId { get; set; }

    [MaxLength(30)] public string OrderCode { get; set; } = ""; // Phieu chi dinh

    public int EncounterId { get; set; }
    public Encounter? Encounter { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    public int Quantity { get; set; } = 1;

    public OrderStatus Status { get; set; } = OrderStatus.Requested;

    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string OrderedBy { get; set; } = ""; // Giu lai cho backward-compat (ten BS)

    public int? OrderedByStaffId { get; set; }
    public Staff? OrderedByStaff { get; set; }

    public DateTime? StartedAt { get; set; }   // KTV bat dau thuc hien
    public DateTime? CompletedAt { get; set; } // Hoàn thành

    public OrderResult? OrderResult { get; set; }
}

public class OrderResult
{
    public int OrderResultId { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    [MaxLength(2000)]
    public string ResultText { get; set; } = "";

    public DateTime ResultedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string ResultedBy { get; set; } = "";

    public int? ResultedByStaffId { get; set; }
    public Staff? ResultedByStaff { get; set; }

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; } // Link anh / PDF
}
