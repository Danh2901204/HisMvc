using System.ComponentModel.DataAnnotations;

namespace HisMvc.Entities;

// 1. Khoa/Bu?ng n?i trú
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

// 2. Gi??ng b?nh
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

// 3. H? s? nh?p vi?n
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
    
    [MaxLength(2000)]
    public string? DischargeSummary { get; set; }
    
    [MaxLength(1000)]
    public string? DischargeInstructions { get; set; }
    
    public int? DischargedBy { get; set; }
    public Staff? DischargedByStaff { get; set; }
    
    public List<MedicalOrder> MedicalOrders { get; set; } = new();
}

// 4. Y l?nh (theo gi? cho b?nh nhân n?i trú)
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
    public Staff? OrderedByStaff { get; set; }
    
    public MedicalOrderStatus Status { get; set; } = MedicalOrderStatus.Ordered;
    
    public DateTime? ExecutedAt { get; set; }
    
    public int? ExecutedBy { get; set; }
    public Staff? ExecutedByStaff { get; set; }
    
    [MaxLength(1000)]
    public string? ExecutionNote { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    // Liên k?t v?i ??n thu?c n?u là y l?nh thu?c
    public int? PrescriptionId { get; set; }
    public Prescription? Prescription { get; set; }
}

// 5. Ph?u thu?t/Th? thu?t
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

// 6. Sinh hi?u (Vital Signs)
public class VitalSign
{
    public int VitalSignId { get; set; }
    
    public int AdmissionId { get; set; }
    public Admission? Admission { get; set; }
    
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    public int RecordedBy { get; set; }
    public Staff? RecordedByStaff { get; set; }
    
    public decimal? Temperature { get; set; } // ?? C
    
    public int? HeartRate { get; set; } // nh?p/phút
    
    public int? RespiratoryRate { get; set; } // nh?p/phút
    
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
