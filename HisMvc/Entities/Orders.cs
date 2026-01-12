using System.ComponentModel.DataAnnotations;

namespace HisMvc.Entities;

public class Service
{
    public int ServiceId { get; set; }
    [MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(50)] public string Type { get; set; } = "LAB"; // LAB/IMAGING/EXAM
    public decimal Price { get; set; }
}

public class Order
{
    public int OrderId { get; set; }

    public int EncounterId { get; set; }
    public Encounter? Encounter { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Requested;

    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string OrderedBy { get; set; } = "";

    // ✅ navigation kết quả (nullable)
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
}
