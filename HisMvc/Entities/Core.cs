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
    [MaxLength(200)] public string FullName { get; set; } = "";
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    [MaxLength(50)] public string StaffType { get; set; } = "DOCTOR";
}

public class Patient
{
    public int PatientId { get; set; }
    [MaxLength(200)] public string FullName { get; set; } = "";
    public DateOnly? Dob { get; set; }
    public Gender Gender { get; set; } = Gender.Unknown;
    [MaxLength(20)] public string Phone { get; set; } = "";

    // Thông tin BHYT
    [MaxLength(15)] 
    public string? InsuranceNumber { get; set; } // Mã thẻ BHYT (15 ký tự)

    public DateTime? InsuranceExpiry { get; set; } // Ngày hết hạn thẻ

    [MaxLength(10)] 
    public string? InsuranceType { get; set; } // Loại thẻ: QN (Quân nhân), KC (Khám chữa bệnh)...

    public decimal InsuranceCoveragePercent { get; set; } = 0; // % chi trả (80%, 95%, 100%)

    [MaxLength(200)] 
    public string? InsuranceHospital { get; set; } // Nơi đăng ký KCB ban đầu

    [MaxLength(200)]
    public string? Address { get; set; } // Địa chỉ (cần cho BHYT)

    [MaxLength(20)]
    public string? IdentityNumber { get; set; } // CMND/CCCD
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
}

public class Encounter
{
    public int EncounterId { get; set; }

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public int DoctorId { get; set; }
    public Staff? Doctor { get; set; }

    public EncounterStatus Status { get; set; } = EncounterStatus.CheckedIn;
    public DateTime CheckInAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)] public string? Diagnosis { get; set; }
    [MaxLength(1000)] public string? Conclusion { get; set; }
    public DateTime EndAt { get; internal set; }
}
