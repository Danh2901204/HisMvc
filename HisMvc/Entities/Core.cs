using System;
using System.ComponentModel.DataAnnotations;

namespace HisMvc.Entities;

public class Department
{
    public int DepartmentId { get; set; }
    [MaxLength(50)] public string Code { get; set; } = "";
    [MaxLength(200)] public string Name { get; set; } = "";
}

public class Staff
{
    public int StaffId { get; set; }
    [MaxLength(30)] public string StaffCode { get; set; } = ""; // Mã NV (NV001)
    [MaxLength(200)] public string FullName { get; set; } = "";
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    [MaxLength(50)] public string StaffType { get; set; } = "DOCTOR";

    // Chung chi hanh nghe (CCHN) - bat buoc cho BS / DS / KTV theo TT 41/2011
    [MaxLength(50)] public string? LicenseNumber { get; set; }
    [MaxLength(200)] public string? Title { get; set; } // Hoc ham/hoc vi: BS, BSCKI, ThS.BS, ...

    public bool IsActive { get; set; } = true;
}

public class Patient
{
    public int PatientId { get; set; }

    [MaxLength(30)]
    public string PatientCode { get; set; } = ""; // Mã BN (BN000123)

    [MaxLength(200)] public string FullName { get; set; } = "";
    public DateOnly? Dob { get; set; }
    public Gender Gender { get; set; } = Gender.Unknown;
    [MaxLength(20)] public string Phone { get; set; } = "";

    // Thông tin HSBA theo TT 56/2017
    [MaxLength(50)] public string? Ethnicity { get; set; }   // Dan toc (Kinh, Tay, ...)
    [MaxLength(100)] public string? Occupation { get; set; } // Nghe nghiep

    // Thông tin BHYT
    [MaxLength(15)]
    public string? InsuranceNumber { get; set; } // Mã thẻ BHYT (15 ký tự)

    public DateTime? InsuranceExpiry { get; set; } // Ngày hết hạn thẻ

    public DateTime? InsuranceValidFrom { get; set; } // Ngay co hieu lúc (5-nam-lien-tuc)

    [MaxLength(10)]
    public string? InsuranceType { get; set; } // Loại thẻ: QN (Quân nhân), KC (Khám chữa bệnh)...

    public decimal InsuranceCoveragePercent { get; set; } = 0; // % chi trả (80%, 95%, 100%)

    [MaxLength(200)]
    public string? InsuranceHospital { get; set; } // Nơi đăng ký KCB ban đầu

    [MaxLength(10)]
    public string? InsuranceHospitalCode { get; set; } // Mã noi DKBD (5-6 ky tu BHYT)

    [MaxLength(200)]
    public string? Address { get; set; } // Địa chỉ (cần cho BHYT)

    [MaxLength(20)]
    public string? IdentityNumber { get; set; } // CMND/CCCD

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class TimeSlot
{
    public int TimeSlotId { get; set; }
    [MaxLength(20)] public string Code { get; set; } = "";
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
}

public class Appointment
{
    public int AppointmentId { get; set; }
    [MaxLength(30)] public string Code { get; set; } = "";

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? DoctorId { get; set; }
    public Staff? Doctor { get; set; }

    public DateOnly Date { get; set; }
    public int TimeSlotId { get; set; }
    public TimeSlot? TimeSlot { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    [MaxLength(500)] public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CheckedInAt { get; set; } // Thoi diem bệnh nhân check-in tai le tan
}

public class Encounter
{
    public int EncounterId { get; set; }

    [MaxLength(30)]
    public string EncounterCode { get; set; } = ""; // Mã kham (LK20260525-001)

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public int DoctorId { get; set; }
    public Staff? Doctor { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public EncounterStatus Status { get; set; } = EncounterStatus.CheckedIn;
    public DateTime CheckInAt { get; set; } = DateTime.UtcNow;

    // STT / hang doi phòng khám (buoc 3 trong luong)
    public int? QueueNumber { get; set; }     // STT gọi vào khám
    [MaxLength(20)] public string? RoomNumber { get; set; }  // Phong kham duoc xep

    // Thoi diem cap STT (sau khi thu phi kham)
    public DateTime? QueuedAt { get; set; }

    // Thoi diem BS gọi vào phòng (bat dau kham)
    public DateTime? StartedAt { get; set; }

    [MaxLength(500)] public string? Diagnosis { get; set; }

    // ICD-10 theo TT 56/2017 - bat buoc cho BHYT
    [MaxLength(10)] public string? Icd10Primary { get; set; }   // Mã benh chinh
    [MaxLength(50)] public string? Icd10Secondary { get; set; } // Mã benh kem theo (cach nhau ,)
    [MaxLength(200)] public string? Icd10PrimaryName { get; set; } // Tên benh chinh

    [MaxLength(1000)] public string? Conclusion { get; set; }
    [MaxLength(1000)] public string? Instructions { get; set; } // Dan do BN
    public DateOnly? FollowUpDate { get; set; } // Hen tai kham

    /// <summary>SQL: DATETIME2 NOT NULL — mặc định = lúc check-in, cập nhật khi kết thúc kham.</summary>
    public DateTime EndAt { get; set; }
}
