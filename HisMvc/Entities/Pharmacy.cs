using System.ComponentModel.DataAnnotations;

namespace HisMvc.Entities;

// 1. Thu?c
public class Medicine
{
    public int MedicineId { get; set; }
    
    [MaxLength(50)]
    public string Code { get; set; } = "";
    
    [MaxLength(200)]
    public string Name { get; set; } = "";
    
    [MaxLength(200)]
    public string ActiveIngredient { get; set; } = "";
    
    [MaxLength(50)]
    public string Unit { get; set; } = ""; // Viên, H?p, Chai, Tuýp
    
    [MaxLength(200)]
    public string? Manufacturer { get; set; }
    
    public bool RequiresPrescription { get; set; } = true;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
}

// 2. Lô thu?c (qu?n lý theo h?n dùng và giá nh?p)
public class MedicineBatch
{
    public int MedicineBatchId { get; set; }
    
    public int MedicineId { get; set; }
    public Medicine? Medicine { get; set; }
    
    [MaxLength(50)]
    public string BatchNumber { get; set; } = "";
    
    public DateTime ManufactureDate { get; set; }
    
    public DateTime ExpiryDate { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public int QuantityInStock { get; set; }
    
    public int MinStockLevel { get; set; } = 10; // C?nh báo khi t?n kho th?p
    
    public bool IsActive { get; set; } = true;
}

// 3. ??n thu?c
public class Prescription
{
    public int PrescriptionId { get; set; }
    
    [MaxLength(30)]
    public string Code { get; set; } = "";
    
    public int EncounterId { get; set; }
    public Encounter? Encounter { get; set; }
    
    public int PrescribedBy { get; set; }
    public Staff? Doctor { get; set; }
    
    public DateTime PrescribedAt { get; set; } = DateTime.UtcNow;
    
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    public List<PrescriptionItem> Items { get; set; } = new();
}

// 4. Chi ti?t ??n thu?c
public class PrescriptionItem
{
    public int PrescriptionItemId { get; set; }
    
    public int PrescriptionId { get; set; }
    public Prescription? Prescription { get; set; }
    
    public int MedicineId { get; set; }
    public Medicine? Medicine { get; set; }
    
    public int Quantity { get; set; }
    
    [MaxLength(200)]
    public string Dosage { get; set; } = ""; // "2 viên x 3 l?n/ngày"
    
    [MaxLength(500)]
    public string? Instructions { get; set; } // "U?ng sau ?n"
    
    public int Duration { get; set; } = 1; // S? ngày dùng thu?c
}

// 5. C?p phát thu?c
public class PharmacyDispense
{
    public int PharmacyDispenseId { get; set; }
    
    public int PrescriptionId { get; set; }
    public Prescription? Prescription { get; set; }
    
    public DateTime DispensedAt { get; set; } = DateTime.UtcNow;
    
    public int DispensedBy { get; set; }
    public Staff? Pharmacist { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    public List<DispenseItem> Items { get; set; } = new();
}

// 6. Chi ti?t c?p phát (ghi l?i lô thu?c ?ã c?p)
public class DispenseItem
{
    public int DispenseItemId { get; set; }
    
    public int PharmacyDispenseId { get; set; }
    public PharmacyDispense? PharmacyDispense { get; set; }
    
    public int MedicineBatchId { get; set; }
    public MedicineBatch? MedicineBatch { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal TotalPrice { get; set; }
}

// 7. Qu?n lý kho (nh?p/xu?t/?i?u ch?nh)
public class InventoryTransaction
{
    public int InventoryTransactionId { get; set; }
    
    [MaxLength(30)]
    public string TransactionCode { get; set; } = "";
    
    public int MedicineBatchId { get; set; }
    public MedicineBatch? MedicineBatch { get; set; }
    
    public TransactionType Type { get; set; }
    
    public int Quantity { get; set; } // + cho nh?p, - cho xu?t
    
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    public int? CreatedBy { get; set; }
    public Staff? Staff { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    [MaxLength(100)]
    public string? ReferenceCode { get; set; } // Mã phi?u nh?p/xu?t
}
