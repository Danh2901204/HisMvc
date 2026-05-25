namespace HisMvc.Entities;

public enum Gender { Unknown = 0, Male = 1, Female = 2, Other = 3 }
public enum AppointmentStatus { Booked = 1, CheckedIn = 2, Completed = 8, Cancelled = 9, NoShow = 10 }

// Luong KCB ngoai tru (theo luong.txt - chuan 10 buoc):
// 1. CheckedIn          - Đã check-in tai le tan, cho thu phi kham
// 2. WaitingExam        - Đã thu phi kham, Đang xếp hàng STT cho vào phòng
// 3. InService          - Đang kham (BS Đang làm việc với BN)
// 4. WaitingResult      - BS da chi dinh CLS, cho ket qua
// 5. WaitingFinalPayment- BS da ket luan + ke don, cho thu chi phí phát sinh
// 6. WaitingMedicine    - Đã thu tien, cho linh thuốc tai nha thuốc
// 7. Completed          - Hoàn thành
// 8. Cancelled          - Hủy
public enum EncounterStatus
{
    CheckedIn = 1,
    WaitingExam = 2,
    InService = 3,
    WaitingResult = 4,
    WaitingFinalPayment = 5,
    WaitingMedicine = 6,
    Completed = 8,
    Cancelled = 9
}

public enum OrderStatus { Requested = 1, InProgress = 3, Resulted = 6, Verified = 7, Cancelled = 9 }

// Pharmacy Enums
public enum PrescriptionStatus { Pending = 1, Dispensed = 2, Cancelled = 9 }
public enum TransactionType { Import = 1, Export = 2, Adjustment = 3 }

// Inpatient Enums
public enum WardType { General = 1, VIP = 2, ICU = 3, Emergency = 4 }
public enum BedStatus { Empty = 1, Occupied = 2, Cleaning = 3, Maintenance = 4, Reserved = 5 }
public enum AdmissionStatus { Active = 1, Discharged = 8, Transferred = 7, Cancelled = 9 }
public enum MedicalOrderType { Medication = 1, Procedure = 2, Diet = 3, Nursing = 4, Lab = 5, Imaging = 6 }
public enum MedicalOrderStatus { Ordered = 1, InProgress = 2, Completed = 8, Cancelled = 9 }
public enum SurgeryStatus { Scheduled = 1, InProgress = 2, Completed = 8, Cancelled = 9, Postponed = 7 }
public enum AllergySeverity { Mild = 1, Moderate = 2, Severe = 3 }

// Insurance Enums
public enum ClaimStatus { Pending = 1, Submitted = 2, Approved = 6, Rejected = 9, PartiallyApproved = 5 }

// Loai hóa đơn - tach 2 dot thu (theo luong moi)
public enum InvoiceType
{
    ExamFee = 1,   // Phí khám ban đầu (thu trước khi vào phòng)
    Services = 2,  // Phi CLS (chi dinh xet nghiem / chẩn đoán hinh anh)
    Medicine = 3,  // Tiền thuốc
    Final = 4,     // Hoa don tong hop cuoi (services + medicine sau khi BS ket luan)
    Inpatient = 5  // Hoa don tong hop noi tru khi xuất viện
}
