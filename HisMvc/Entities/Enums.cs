namespace HisMvc.Entities;

public enum Gender { Unknown = 0, Male = 1, Female = 2, Other = 3 }
public enum AppointmentStatus { Booked = 1, Cancelled = 9 }
public enum EncounterStatus { CheckedIn = 1, InService = 2, Completed = 8, Cancelled = 9 }
public enum OrderStatus { Requested = 1, Resulted = 6, Verified = 7, Cancelled = 9 }

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

