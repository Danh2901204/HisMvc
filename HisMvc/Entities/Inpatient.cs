using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HisMvc.Entities;

// 1. Khoa/Buồng noi tru
public class Ward
{
    public int WardId { get; set; }
    
    [MaxLength(50)]
    public string Code { get; set; } = "";
    
    [MaxLength(200)]
    public string Name { get; set; } = "";
    
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    
    public WardType Type { get; set; } = WardType.General;
    
    public int TotalBeds { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

// 2. Giường bệnh
public class Bed
{
    public int BedId { get; set; }
    
    [MaxLength(20)]
    public string BedNumber { get; set; } = "";
    
    public int WardId { get; set; }
    public Ward? Ward { get; set; }
    
    public BedStatus Status { get; set; } = BedStatus.Empty;
    
    public bool IsActive { get; set; } = true;
    
    [MaxLength(500)]
    public string? Note { get; set; }
}

// 3. Ho so nhập viện
public class Admission
{
    public int AdmissionId { get; set; }
    
    [MaxLength(30)]
    public string AdmissionCode { get; set; } = "";
    
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    
    public int BedId { get; set; }
    public Bed? Bed { get; set; }
    
    public int AttendingDoctorId { get; set; }
    public Staff? AttendingDoctor { get; set; }
    
    public DateTime AdmittedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? DischargedAt { get; set; }
    
    public AdmissionStatus Status { get; set; } = AdmissionStatus.Active;
    
    [MaxLength(1000)]
    public string AdmissionReason { get; set; } = "";

    [MaxLength(1000)]
    public string? InitialDiagnosis { get; set; }

    // ICD-10 ngày vào viện (TT 56/2017)
    [MaxLength(10)] public string? Icd10Admission { get; set; }
    // ICD-10 ngày ra vien
    [MaxLength(10)] public string? Icd10Discharge { get; set; }
    [MaxLength(50)] public string? Icd10DischargeSecondary { get; set; }

    [MaxLength(2000)]
    public string? DischargeSummary { get; set; }

    [MaxLength(1000)]
    public string? DischargeInstructions { get; set; }

    // Phân loại theo TT 56/2017: 1=Khoi, 2=Do, 3=Không doi, 4=Nặng, 5=Tử vong
    public int? DischargeResult { get; set; }

    public int? DischargedBy { get; set; }

    [ForeignKey(nameof(DischargedBy))]
    public Staff? DischargedByStaff { get; set; }

    public List<MedicalOrder> MedicalOrders { get; set; } = new();
}

// 4. Lệnh điều trị (theo giờ cho bệnh nhân nội trú)
public class MedicalOrder
{
    public int MedicalOrderId { get; set; }
    
    [MaxLength(30)]
    public string OrderCode { get; set; } = "";
    
    public int AdmissionId { get; set; }
    public Admission? Admission { get; set; }
    
    public MedicalOrderType OrderType { get; set; }
    
    [MaxLength(1000)]
    public string OrderDetails { get; set; } = "";
    
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ScheduledAt { get; set; }
    
    public int OrderedBy { get; set; }

    [ForeignKey(nameof(OrderedBy))]
    public Staff? OrderedByStaff { get; set; }
    
    public MedicalOrderStatus Status { get; set; } = MedicalOrderStatus.Ordered;
    
    public DateTime? ExecutedAt { get; set; }
    
    public int? ExecutedBy { get; set; }

    [ForeignKey(nameof(ExecutedBy))]
    public Staff? ExecutedByStaff { get; set; }
    
    [MaxLength(1000)]
    public string? ExecutionNote { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    // Liên kết với đơn thuốc nếu là lệnh thuốc
    public int? PrescriptionId { get; set; }
    public Prescription? Prescription { get; set; }
}

// 5. Phẩu thuật/Thu thuat
public class Surgery
{
    public int SurgeryId { get; set; }
    
    [MaxLength(30)]
    public string SurgeryCode { get; set; } = "";
    
    public int AdmissionId { get; set; }
    public Admission? Admission { get; set; }
    
    [MaxLength(500)]
    public string SurgeryType { get; set; } = "";
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public DateTime ScheduledAt { get; set; }
    
    public int SurgeonId { get; set; }
    public Staff? Surgeon { get; set; }
    
    public int? AnesthesiologistId { get; set; }
    public Staff? Anesthesiologist { get; set; }
    
    [MaxLength(200)]
    public string? OperatingRoom { get; set; }
    
    public SurgeryStatus Status { get; set; } = SurgeryStatus.Scheduled;
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? EndedAt { get; set; }
    
    [MaxLength(2000)]
    public string? OperativeNotes { get; set; }
    
    [MaxLength(1000)]
    public string? PostOpInstructions { get; set; }
    
    [MaxLength(500)]
    public string? Complications { get; set; }
}

// 6. Sinh hiệu (Vital Signs)
public class VitalSign
{
    public int VitalSignId { get; set; }

    public int AdmissionId { get; set; }
    public Admission? Admission { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Ca ghi: 1=Sang, 2=Chieu, 3=Toi (theo MoH 3 lan/ngày)
    public int Shift { get; set; } = 1;

    public int RecordedBy { get; set; }

    [ForeignKey(nameof(RecordedBy))]
    public Staff? RecordedByStaff { get; set; }
    
    public decimal? Temperature { get; set; } // do C

    public int? HeartRate { get; set; } // nhip/phút

    public int? RespiratoryRate { get; set; } // nhip/phút
    
    public int? BloodPressureSystolic { get; set; } // mmHg
    
    public int? BloodPressureDiastolic { get; set; } // mmHg
    
    public decimal? OxygenSaturation { get; set; } // %
    
    public decimal? Weight { get; set; } // kg
    
    public decimal? Height { get; set; } // cm
    
    [MaxLength(500)]
    public string? Note { get; set; }
}

// 7. D? ?ng (cho EMR)
public class Allergy
{
    public int AllergyId { get; set; }

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    [MaxLength(200)]
    public string Allergen { get; set; } = "";

    // 1=Thuốc, 2=Thuc pham, 3=Moi truong, 4=Khac
    public int AllergyType { get; set; } = 1;

    [MaxLength(500)]
    public string? Reaction { get; set; }

    public AllergySeverity Severity { get; set; } = AllergySeverity.Moderate;

    public DateTime IdentifiedDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Note { get; set; }
}

// 8. Ti?n s? b?nh
public class MedicalHistory
{
    public int MedicalHistoryId { get; set; }
    
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    
    [MaxLength(500)]
    public string Condition { get; set; } = "";
    
    public DateTime DiagnosedDate { get; set; }
    
    [MaxLength(1000)]
    public string? Treatment { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [MaxLength(500)]
    public string? Note { get; set; }
}
