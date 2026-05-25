using System.ComponentModel.DataAnnotations;

namespace HisMvc.Entities;

// 1. Thuốc
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
    public string Unit { get; set; } = ""; // Vien, Hop, Chai, Tuyp

    [MaxLength(200)]
    public string? Manufacturer { get; set; }

    public bool RequiresPrescription { get; set; } = true;

    // BHYT theo TT 30/2018 + QD 130/BHXH
    [MaxLength(20)] public string? BhytCode { get; set; } // Mã thuốc BHYT
    public bool IsInBhytList { get; set; } = true;
    public decimal? BhytPrice { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

// 2. Lô thuốc (quản lý theo hạn dùng và giá nhập)
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
    
    public int MinStockLevel { get; set; } = 10; // Cảnh báo khi ton kho thap
    
    public bool IsActive { get; set; } = true;
}

// 3. Don thuốc
public class Prescription
{
    public int PrescriptionId { get; set; }

    [MaxLength(30)]
    public string Code { get; set; } = "";

    // Mot trong hai phai co - encounter cho ngoai tru, admission cho noi tru
    public int? EncounterId { get; set; }
    public Encounter? Encounter { get; set; }

    public int? AdmissionId { get; set; }
    public Admission? Admission { get; set; }

    public int PrescribedBy { get; set; }
    public Staff? Doctor { get; set; }

    public DateTime PrescribedAt { get; set; } = DateTime.UtcNow;

    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;

    [MaxLength(500)]
    public string? Note { get; set; }

    public List<PrescriptionItem> Items { get; set; } = new();
}

// 4. Chi tiet đơn thuốc
public class PrescriptionItem
{
    public int PrescriptionItemId { get; set; }
    
    public int PrescriptionId { get; set; }
    public Prescription? Prescription { get; set; }
    
    public int MedicineId { get; set; }
    public Medicine? Medicine { get; set; }
    
    public int Quantity { get; set; }
    
    [MaxLength(200)]
    public string Dosage { get; set; } = ""; // "2 vien x 3 lan/ngày"
    
    [MaxLength(500)]
    public string? Instructions { get; set; } // "Uong sau an"
    
    public int Duration { get; set; } = 1; // So ngày dung thuốc
}

// 5. Cấp phát thuốc
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

// 6. Chi tiet cấp phát (ghi lai lo thuốc đã cấp)
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

// 7. Quản lý kho (nhap/xuat/dieu chinh)
public class InventoryTransaction
{
    public int InventoryTransactionId { get; set; }
    
    [MaxLength(30)]
    public string TransactionCode { get; set; } = "";
    
    public int MedicineBatchId { get; set; }
    public MedicineBatch? MedicineBatch { get; set; }
    
    public TransactionType Type { get; set; }
    
    public int Quantity { get; set; } // + cho nhập, - cho xuất
    
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    public int? CreatedBy { get; set; }
    public int? StaffId { get; set; }
    public Staff? Staff { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    [MaxLength(100)]
    public string? ReferenceCode { get; set; } // Mã phiếu nhập/xuat
}
