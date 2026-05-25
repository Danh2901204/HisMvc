# ??? C? S? D? LI?U HIS MANAGEMENT SYSTEM

## ?? T?NG QUAN

**Database Engine:** SQL Server  
**Total Tables:** 31  
**Total Relationships:** 40+  
**Framework:** Entity Framework Core 10.0

---

## ?? DANH SÁCH B?NG THEO MODULE

### 1. CORE MODULE (5 tables)

#### `Patients` - Thông tin b?nh nhân
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| PatientId | int | PK, Identity | ID b?nh nhân |
| FullName | nvarchar(200) | NOT NULL | H? tên |
| Dob | date | NULL | Ngày sinh |
| Gender | int | NOT NULL | Gi?i tính (0=Unknown, 1=Male, 2=Female, 3=Other) |
| Phone | nvarchar(20) | NOT NULL | S? ?i?n tho?i |
| Address | nvarchar(200) | NULL | ??a ch? |
| IdentityNumber | nvarchar(20) | NULL | CMND/CCCD |
| **InsuranceNumber** | nvarchar(15) | NULL | Mã th? BHYT |
| **InsuranceExpiry** | datetime2 | NULL | Ngày h?t h?n BHYT |
| **InsuranceType** | nvarchar(10) | NULL | Lo?i th? BHYT |
| **InsuranceCoveragePercent** | decimal(5,2) | DEFAULT 0 | % chi tr? BHYT |
| **InsuranceHospital** | nvarchar(200) | NULL | N?i ??ng ký KCB |

**Indexes:**
- PK_Patients (PatientId)

---

#### `Departments` - Khoa/Phòng ban
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| DepartmentId | int | PK, Identity | ID khoa |
| Code | nvarchar(20) | NOT NULL, UNIQUE | Mã khoa |
| Name | nvarchar(200) | NOT NULL | Tên khoa |

**Indexes:**
- PK_Departments (DepartmentId)
- IX_Departments_Code (Code) UNIQUE

---

#### `Staffs` - Nhân viên y t?
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| StaffId | int | PK, Identity | ID nhân viên |
| FullName | nvarchar(200) | NOT NULL | H? tên |
| DepartmentId | int | FK | Khoa |
| StaffType | nvarchar(50) | NOT NULL | Lo?i (DOCTOR, NURSE...) |

**Foreign Keys:**
- FK_Staffs_Departments (DepartmentId ? Departments.DepartmentId)

---

#### `TimeSlots` - Khung gi? khám
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| TimeSlotId | int | PK, Identity | ID khung gi? |
| Code | nvarchar(20) | NOT NULL, UNIQUE | Mã (S1, S2...) |
| Start | time | NOT NULL | Gi? b?t ??u |
| End | time | NOT NULL | Gi? k?t thúc |

**Indexes:**
- IX_TimeSlots_Code (Code) UNIQUE

---

#### `Services` - D?ch v? y t?
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| ServiceId | int | PK, Identity | ID d?ch v? |
| Name | nvarchar(200) | NOT NULL | Tên d?ch v? |
| Type | nvarchar(50) | NOT NULL | Lo?i (LAB, IMAGING...) |
| Price | decimal(18,2) | NOT NULL | Giá |

---

### 2. APPOINTMENT MODULE (2 tables)

#### `Appointments` - L?ch h?n khám
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| AppointmentId | int | PK, Identity | ID l?ch h?n |
| PatientId | int | FK, NOT NULL | B?nh nhân |
| DoctorId | int | FK, NOT NULL | Bác s? |
| TimeSlotId | int | FK, NOT NULL | Khung gi? |
| AppointmentDate | date | NOT NULL | Ngày h?n |
| Status | int | NOT NULL | Tr?ng thái (1=Booked, 9=Cancelled) |
| Note | nvarchar(500) | NULL | Ghi chú |

**Foreign Keys:**
- FK_Appointments_Patients (PatientId ? Patients.PatientId)
- FK_Appointments_Staffs_Doctor (DoctorId ? Staffs.StaffId)
- FK_Appointments_TimeSlots (TimeSlotId ? TimeSlots.TimeSlotId)

---

#### `Encounters` - L??t khám b?nh
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| EncounterId | int | PK, Identity | ID l??t khám |
| PatientId | int | FK, NOT NULL | B?nh nhân |
| DoctorId | int | FK, NOT NULL | Bác s? |
| AppointmentId | int | FK, NULL | L?ch h?n (n?u có) |
| CheckInAt | datetime2 | NOT NULL | Th?i gian check-in |
| Status | int | NOT NULL | Tr?ng thái (1=CheckedIn, 2=InService, 8=Completed, 9=Cancelled) |
| Diagnosis | nvarchar(1000) | NULL | Ch?n ?oán |
| Conclusion | nvarchar(1000) | NULL | K?t lu?n |
| EndAt | datetime2 | NULL | Th?i gian k?t thúc |

**Foreign Keys:**
- FK_Encounters_Patients (PatientId ? Patients.PatientId)
- FK_Encounters_Staffs (DoctorId ? Staffs.StaffId)
- FK_Encounters_Appointments (AppointmentId ? Appointments.AppointmentId)

---

### 3. ORDER & LAB MODULE (3 tables)

#### `Orders` - Ch? ??nh d?ch v?
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| OrderId | int | PK, Identity | ID ch? ??nh |
| EncounterId | int | FK, NOT NULL | L??t khám |
| ServiceId | int | FK, NOT NULL | D?ch v? |
| Status | int | NOT NULL | Tr?ng thái (1=Requested, 6=Resulted, 7=Verified, 9=Cancelled) |
| OrderedBy | nvarchar(200) | NOT NULL | Ng??i ch? ??nh |
| OrderedAt | datetime2 | NOT NULL | Th?i gian ch? ??nh |

**Foreign Keys:**
- FK_Orders_Encounters (EncounterId ? Encounters.EncounterId)
- FK_Orders_Services (ServiceId ? Services.ServiceId)

---

#### `OrderResults` - K?t qu? ch? ??nh
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| OrderResultId | int | PK, Identity | ID k?t qu? |
| OrderId | int | FK, NOT NULL, UNIQUE | Ch? ??nh |
| Result | nvarchar(2000) | NULL | K?t qu? |
| PerformedBy | nvarchar(200) | NULL | Ng??i th?c hi?n |
| PerformedAt | datetime2 | NOT NULL | Th?i gian th?c hi?n |
| VerifiedBy | nvarchar(200) | NULL | Ng??i duy?t |
| VerifiedAt | datetime2 | NULL | Th?i gian duy?t |

**Foreign Keys:**
- FK_OrderResults_Orders (OrderId ? Orders.OrderId) UNIQUE

---

### 4. INVOICE MODULE (1 table)

#### `Invoices` - Hóa ??n
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| InvoiceId | int | PK, Identity | ID hóa ??n |
| EncounterId | int | FK, NOT NULL | L??t khám |
| InvoiceCode | nvarchar(50) | NOT NULL, UNIQUE | Mã hóa ??n |
| TotalAmount | decimal(18,2) | NOT NULL | T?ng ti?n |
| **InsuranceAmount** | decimal(18,2) | DEFAULT 0 | BHYT chi tr? |
| **PatientAmount** | decimal(18,2) | DEFAULT 0 | B?nh nhân tr? |
| **HasInsurance** | bit | DEFAULT 0 | Có dùng BHYT |
| **InsuranceClaimId** | int | FK, NULL | ID giám ??nh |
| Status | int | NOT NULL | Tr?ng thái (1=Unpaid, 2=Paid) |
| CreatedAt | datetime2 | NOT NULL | Ngày t?o |
| PaidAt | datetime2 | NULL | Ngày thanh toán |
| PaidBy | nvarchar(200) | NULL | Ng??i thu ti?n |
| Note | nvarchar(500) | NULL | Ghi chú |
| **TaxCode** | nvarchar(100) | NULL | MST |
| **EInvoiceCode** | nvarchar(200) | NULL | Mã H? ?i?n t? |
| **EInvoiceIssuedAt** | datetime2 | NULL | Ngày xu?t H? ?i?n t? |

**Indexes:**
- IX_Invoices_InvoiceCode (InvoiceCode) UNIQUE

**Foreign Keys:**
- FK_Invoices_Encounters (EncounterId ? Encounters.EncounterId)
- FK_Invoices_InsuranceClaims (InsuranceClaimId ? InsuranceClaims.InsuranceClaimId)

---

### 5. PHARMACY MODULE (7 tables)

#### `Medicines` - Danh m?c thu?c
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| MedicineId | int | PK, Identity | ID thu?c |
| Code | nvarchar(50) | NOT NULL, UNIQUE | Mã thu?c |
| Name | nvarchar(200) | NOT NULL | Tên thu?c |
| ActiveIngredient | nvarchar(200) | NOT NULL | Ho?t ch?t |
| Unit | nvarchar(50) | NOT NULL | ??n v? (Viên, H?p...) |
| Manufacturer | nvarchar(200) | NULL | Nhà s?n xu?t |
| RequiresPrescription | bit | NOT NULL | C?n ??n thu?c |
| Description | nvarchar(500) | NULL | Mô t? |
| IsActive | bit | NOT NULL | ?ang s? d?ng |

**Indexes:**
- IX_Medicines_Code (Code) UNIQUE

---

#### `MedicineBatches` - Lô thu?c
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| MedicineBatchId | int | PK, Identity | ID lô thu?c |
| MedicineId | int | FK, NOT NULL | Thu?c |
| BatchNumber | nvarchar(50) | NOT NULL | S? lô |
| ManufactureDate | date | NOT NULL | Ngày s?n xu?t |
| ExpiryDate | date | NOT NULL | H?n dùng |
| UnitPrice | decimal(18,2) | NOT NULL | ??n giá |
| QuantityInStock | int | NOT NULL | T?n kho |
| MinStockLevel | int | DEFAULT 10 | M?c t?n t?i thi?u |
| IsActive | bit | NOT NULL | ?ang dùng |

**Foreign Keys:**
- FK_MedicineBatches_Medicines (MedicineId ? Medicines.MedicineId)

---

#### `Prescriptions` - ??n thu?c
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| PrescriptionId | int | PK, Identity | ID ??n thu?c |
| Code | nvarchar(30) | NOT NULL, UNIQUE | Mã ??n thu?c |
| EncounterId | int | FK, NOT NULL | L??t khám |
| PrescribedBy | int | FK, NOT NULL | Bác s? kê ??n |
| PrescribedAt | datetime2 | NOT NULL | Th?i gian kê ??n |
| Status | int | NOT NULL | Tr?ng thái (1=Pending, 2=Dispensed, 9=Cancelled) |
| Note | nvarchar(500) | NULL | Ghi chú |

**Indexes:**
- IX_Prescriptions_Code (Code) UNIQUE

**Foreign Keys:**
- FK_Prescriptions_Encounters (EncounterId ? Encounters.EncounterId)
- FK_Prescriptions_Staffs (PrescribedBy ? Staffs.StaffId)

---

#### `PrescriptionItems` - Chi ti?t ??n thu?c
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| PrescriptionItemId | int | PK, Identity | ID chi ti?t |
| PrescriptionId | int | FK, NOT NULL | ??n thu?c |
| MedicineId | int | FK, NOT NULL | Thu?c |
| Quantity | int | NOT NULL | S? l??ng |
| Dosage | nvarchar(200) | NOT NULL | Li?u dùng |
| Instructions | nvarchar(500) | NULL | H??ng d?n |
| Duration | int | DEFAULT 1 | S? ngày dùng |

**Foreign Keys:**
- FK_PrescriptionItems_Prescriptions (PrescriptionId ? Prescriptions.PrescriptionId) CASCADE
- FK_PrescriptionItems_Medicines (MedicineId ? Medicines.MedicineId)

---

#### `PharmacyDispenses` - Phi?u c?p phát thu?c
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| PharmacyDispenseId | int | PK, Identity | ID c?p phát |
| PrescriptionId | int | FK, NOT NULL | ??n thu?c |
| DispensedAt | datetime2 | NOT NULL | Th?i gian c?p phát |
| DispensedBy | int | FK, NOT NULL | D??c s? |
| Note | nvarchar(500) | NULL | Ghi chú |

**Foreign Keys:**
- FK_PharmacyDispenses_Prescriptions (PrescriptionId ? Prescriptions.PrescriptionId)
- FK_PharmacyDispenses_Staffs (DispensedBy ? Staffs.StaffId)

---

#### `DispenseItems` - Chi ti?t c?p phát
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| DispenseItemId | int | PK, Identity | ID chi ti?t |
| PharmacyDispenseId | int | FK, NOT NULL | Phi?u c?p phát |
| MedicineBatchId | int | FK, NOT NULL | Lô thu?c |
| Quantity | int | NOT NULL | S? l??ng |
| UnitPrice | decimal(18,2) | NOT NULL | ??n giá |
| TotalPrice | decimal(18,2) | NOT NULL | Thành ti?n |

**Foreign Keys:**
- FK_DispenseItems_PharmacyDispenses (PharmacyDispenseId ? PharmacyDispenses.PharmacyDispenseId) CASCADE
- FK_DispenseItems_MedicineBatches (MedicineBatchId ? MedicineBatches.MedicineBatchId)

---

#### `InventoryTransactions` - Giao d?ch kho thu?c
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| InventoryTransactionId | int | PK, Identity | ID giao d?ch |
| TransactionCode | nvarchar(30) | NOT NULL | Mã giao d?ch |
| MedicineBatchId | int | FK, NOT NULL | Lô thu?c |
| Type | int | NOT NULL | Lo?i (1=Import, 2=Export, 3=Adjustment) |
| Quantity | int | NOT NULL | S? l??ng (+/-) |
| TransactionDate | datetime2 | NOT NULL | Ngày giao d?ch |
| CreatedBy | int | FK, NULL | Ng??i t?o |
| Note | nvarchar(500) | NULL | Ghi chú |
| ReferenceCode | nvarchar(100) | NULL | Mã tham chi?u |

**Foreign Keys:**
- FK_InventoryTransactions_MedicineBatches (MedicineBatchId ? MedicineBatches.MedicineBatchId)
- FK_InventoryTransactions_Staffs (CreatedBy ? Staffs.StaffId)

---

### 6. INPATIENT MODULE (8 tables)

#### `Wards` - Bu?ng b?nh
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| WardId | int | PK, Identity | ID bu?ng |
| Code | nvarchar(50) | NOT NULL, UNIQUE | Mã bu?ng |
| Name | nvarchar(200) | NOT NULL | Tên bu?ng |
| DepartmentId | int | FK, NOT NULL | Khoa |
| Type | int | NOT NULL | Lo?i (1=General, 2=VIP, 3=ICU, 4=Emergency) |
| TotalBeds | int | NOT NULL | T?ng s? gi??ng |
| IsActive | bit | NOT NULL | ?ang ho?t ??ng |
| Description | nvarchar(500) | NULL | Mô t? |

**Indexes:**
- IX_Wards_Code (Code) UNIQUE

**Foreign Keys:**
- FK_Wards_Departments (DepartmentId ? Departments.DepartmentId)

---

#### `Beds` - Gi??ng b?nh
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| BedId | int | PK, Identity | ID gi??ng |
| WardId | int | FK, NOT NULL | Bu?ng |
| BedNumber | nvarchar(20) | NOT NULL | S? gi??ng |
| Status | int | NOT NULL | Tr?ng thái (1=Empty, 2=Occupied, 3=Cleaning, 4=Maintenance, 5=Reserved) |
| IsActive | bit | NOT NULL | ?ang dùng |
| Note | nvarchar(500) | NULL | Ghi chú |

**Indexes:**
- IX_Beds_Ward_BedNumber (WardId, BedNumber) UNIQUE

**Foreign Keys:**
- FK_Beds_Wards (WardId ? Wards.WardId)

---

#### `Admissions` - H? s? nh?p vi?n
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| AdmissionId | int | PK, Identity | ID nh?p vi?n |
| AdmissionCode | nvarchar(30) | NOT NULL, UNIQUE | Mã nh?p vi?n |
| PatientId | int | FK, NOT NULL | B?nh nhân |
| BedId | int | FK, NOT NULL | Gi??ng |
| AttendingDoctorId | int | FK, NOT NULL | Bác s? ?i?u tr? |
| AdmittedAt | datetime2 | NOT NULL | Th?i gian nh?p vi?n |
| DischargedAt | datetime2 | NULL | Th?i gian xu?t vi?n |
| Status | int | NOT NULL | Tr?ng thái (1=Active, 7=Transferred, 8=Discharged, 9=Cancelled) |
| AdmissionReason | nvarchar(1000) | NOT NULL | Lý do nh?p vi?n |
| InitialDiagnosis | nvarchar(1000) | NULL | Ch?n ?oán ban ??u |
| DischargeSummary | nvarchar(2000) | NULL | Tóm t?t ra vi?n |
| DischargeInstructions | nvarchar(1000) | NULL | H??ng d?n sau ra vi?n |
| DischargedBy | int | FK, NULL | Ng??i cho ra vi?n |

**Indexes:**
- IX_Admissions_Code (AdmissionCode) UNIQUE

**Foreign Keys:**
- FK_Admissions_Patients (PatientId ? Patients.PatientId)
- FK_Admissions_Beds (BedId ? Beds.BedId)
- FK_Admissions_Staffs_Doctor (AttendingDoctorId ? Staffs.StaffId)
- FK_Admissions_Staffs_Discharged (DischargedBy ? Staffs.StaffId)

---

#### `MedicalOrders` - Y l?nh
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| MedicalOrderId | int | PK, Identity | ID y l?nh |
| OrderCode | nvarchar(30) | NOT NULL | Mã y l?nh |
| AdmissionId | int | FK, NOT NULL | H? s? nh?p vi?n |
| OrderType | int | NOT NULL | Lo?i (1=Medication, 2=Procedure, 3=Diet, 4=Nursing, 5=Lab, 6=Imaging) |
| OrderDetails | nvarchar(1000) | NOT NULL | Chi ti?t y l?nh |
| OrderedAt | datetime2 | NOT NULL | Th?i gian ch? ??nh |
| ScheduledAt | datetime2 | NOT NULL | Th?i gian th?c hi?n |
| OrderedBy | int | FK, NOT NULL | Ng??i ch? ??nh |
| Status | int | NOT NULL | Tr?ng thái (1=Ordered, 2=InProgress, 8=Completed, 9=Cancelled) |
| ExecutedAt | datetime2 | NULL | Th?i gian th?c hi?n |
| ExecutedBy | int | FK, NULL | Ng??i th?c hi?n |
| ExecutionNote | nvarchar(1000) | NULL | Ghi chú th?c hi?n |
| Note | nvarchar(500) | NULL | Ghi chú |
| PrescriptionId | int | FK, NULL | ??n thu?c (n?u là y l?nh thu?c) |

**Foreign Keys:**
- FK_MedicalOrders_Admissions (AdmissionId ? Admissions.AdmissionId) CASCADE
- FK_MedicalOrders_Staffs_Ordered (OrderedBy ? Staffs.StaffId)
- FK_MedicalOrders_Staffs_Executed (ExecutedBy ? Staffs.StaffId)
- FK_MedicalOrders_Prescriptions (PrescriptionId ? Prescriptions.PrescriptionId)

---

#### `Surgeries` - Ph?u thu?t
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| SurgeryId | int | PK, Identity | ID ph?u thu?t |
| SurgeryCode | nvarchar(30) | NOT NULL, UNIQUE | Mã ph?u thu?t |
| AdmissionId | int | FK, NOT NULL | H? s? nh?p vi?n |
| SurgeryType | nvarchar(500) | NOT NULL | Lo?i ph?u thu?t |
| Description | nvarchar(1000) | NULL | Mô t? |
| ScheduledAt | datetime2 | NOT NULL | Th?i gian d? ki?n |
| SurgeonId | int | FK, NOT NULL | Ph?u thu?t viên |
| AnesthesiologistId | int | FK, NULL | Bác s? gây mê |
| OperatingRoom | nvarchar(200) | NULL | Phòng m? |
| Status | int | NOT NULL | Tr?ng thái (1=Scheduled, 2=InProgress, 7=Postponed, 8=Completed, 9=Cancelled) |
| StartedAt | datetime2 | NULL | Gi? b?t ??u |
| EndedAt | datetime2 | NULL | Gi? k?t thúc |
| OperativeNotes | nvarchar(2000) | NULL | Ghi chú ph?u thu?t |
| PostOpInstructions | nvarchar(1000) | NULL | H??ng d?n sau m? |
| Complications | nvarchar(500) | NULL | Bi?n ch?ng |

**Indexes:**
- IX_Surgeries_Code (SurgeryCode) UNIQUE

**Foreign Keys:**
- FK_Surgeries_Admissions (AdmissionId ? Admissions.AdmissionId)
- FK_Surgeries_Staffs_Surgeon (SurgeonId ? Staffs.StaffId)
- FK_Surgeries_Staffs_Anesthesiologist (AnesthesiologistId ? Staffs.StaffId)

---

#### `VitalSigns` - Sinh hi?u
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| VitalSignId | int | PK, Identity | ID sinh hi?u |
| AdmissionId | int | FK, NOT NULL | H? s? nh?p vi?n |
| RecordedAt | datetime2 | NOT NULL | Th?i gian ghi nh?n |
| RecordedBy | int | FK, NOT NULL | Ng??i ghi nh?n |
| Temperature | decimal(4,1) | NULL | Nhi?t ?? (°C) |
| HeartRate | int | NULL | Nh?p tim (nh?p/phút) |
| RespiratoryRate | int | NULL | Nh?p th? (l?n/phút) |
| BloodPressureSystolic | int | NULL | Huy?t áp tâm thu (mmHg) |
| BloodPressureDiastolic | int | NULL | Huy?t áp tâm tr??ng (mmHg) |
| OxygenSaturation | decimal(5,2) | NULL | SpO2 (%) |
| Weight | decimal(5,2) | NULL | Cân n?ng (kg) |
| Height | decimal(5,2) | NULL | Chi?u cao (cm) |
| Note | nvarchar(500) | NULL | Ghi chú |

**Foreign Keys:**
- FK_VitalSigns_Admissions (AdmissionId ? Admissions.AdmissionId) CASCADE
- FK_VitalSigns_Staffs (RecordedBy ? Staffs.StaffId)

---

#### `Allergies` - D? ?ng
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| AllergyId | int | PK, Identity | ID d? ?ng |
| PatientId | int | FK, NOT NULL | B?nh nhân |
| Allergen | nvarchar(200) | NOT NULL | Ch?t gây d? ?ng |
| Reaction | nvarchar(500) | NULL | Ph?n ?ng |
| Severity | int | NOT NULL | M?c ?? (1=Mild, 2=Moderate, 3=Severe) |
| IdentifiedDate | datetime2 | NOT NULL | Ngày phát hi?n |
| IsActive | bit | NOT NULL | Còn hi?u l?c |
| Note | nvarchar(500) | NULL | Ghi chú |

**Foreign Keys:**
- FK_Allergies_Patients (PatientId ? Patients.PatientId) CASCADE

---

#### `MedicalHistories` - Ti?n s? b?nh
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| MedicalHistoryId | int | PK, Identity | ID ti?n s? |
| PatientId | int | FK, NOT NULL | B?nh nhân |
| Condition | nvarchar(500) | NOT NULL | Tình tr?ng |
| DiagnosedDate | datetime2 | NOT NULL | Ngày ch?n ?oán |
| Treatment | nvarchar(1000) | NULL | ?i?u tr? |
| IsActive | bit | NOT NULL | Còn hi?u l?c |
| Note | nvarchar(500) | NULL | Ghi chú |

**Foreign Keys:**
- FK_MedicalHistories_Patients (PatientId ? Patients.PatientId) CASCADE

---

### 7. INSURANCE MODULE (3 tables)

#### `InsuranceClaims` - Giám ??nh BHYT
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| InsuranceClaimId | int | PK, Identity | ID giám ??nh |
| ClaimCode | nvarchar(30) | NOT NULL, UNIQUE | Mã giám ??nh |
| EncounterId | int | FK, NULL | L??t khám |
| AdmissionId | int | FK, NULL | Nh?p vi?n |
| PatientId | int | FK, NOT NULL | B?nh nhân |
| InsuranceNumber | nvarchar(15) | NOT NULL | Mã th? BHYT |
| InsuranceExpiry | datetime2 | NOT NULL | H?n th? |
| InsuranceType | nvarchar(10) | NOT NULL | Lo?i th? |
| CoveragePercent | decimal(5,2) | NOT NULL | % chi tr? |
| TotalAmount | decimal(18,2) | NOT NULL | T?ng chi phí |
| InsuranceCovered | decimal(18,2) | NOT NULL | BHYT chi tr? |
| PatientPayment | decimal(18,2) | NOT NULL | B?nh nhân tr? |
| Status | int | NOT NULL | Tr?ng thái (1=Pending, 2=Submitted, 5=PartiallyApproved, 6=Approved, 9=Rejected) |
| CreatedAt | datetime2 | NOT NULL | Ngày t?o |
| SubmittedAt | datetime2 | NULL | Ngày g?i |
| ApprovedAt | datetime2 | NULL | Ngày duy?t |
| ApprovedBy | int | FK, NULL | Ng??i duy?t |
| Note | nvarchar(500) | NULL | Ghi chú |
| RejectReason | nvarchar(500) | NULL | Lý do t? ch?i |
| XmlData | nvarchar(MAX) | NULL | XML chu?n B? Y t? |

**Indexes:**
- IX_InsuranceClaims_Code (ClaimCode) UNIQUE

**Foreign Keys:**
- FK_InsuranceClaims_Patients (PatientId ? Patients.PatientId)
- FK_InsuranceClaims_Encounters (EncounterId ? Encounters.EncounterId)
- FK_InsuranceClaims_Admissions (AdmissionId ? Admissions.AdmissionId)
- FK_InsuranceClaims_Staffs (ApprovedBy ? Staffs.StaffId)

---

#### `InsuranceClaimItems` - Chi ti?t giám ??nh
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| InsuranceClaimItemId | int | PK, Identity | ID chi ti?t |
| InsuranceClaimId | int | FK, NOT NULL | Giám ??nh |
| ServiceName | nvarchar(200) | NOT NULL | Tên d?ch v? |
| ServiceCode | nvarchar(50) | NOT NULL | Mã d?ch v? BHYT |
| Quantity | int | NOT NULL | S? l??ng |
| UnitPrice | decimal(18,2) | NOT NULL | ??n giá |
| TotalPrice | decimal(18,2) | NOT NULL | Thành ti?n |
| InsurancePaid | decimal(18,2) | NOT NULL | BHYT tr? |
| PatientPaid | decimal(18,2) | NOT NULL | BN tr? |
| IsInInsuranceList | bit | NOT NULL | Trong danh m?c BHYT |
| Note | nvarchar(500) | NULL | Ghi chú |

**Foreign Keys:**
- FK_InsuranceClaimItems_InsuranceClaims (InsuranceClaimId ? InsuranceClaims.InsuranceClaimId) CASCADE

---

#### `InsuranceConfigs` - C?u hình BHYT
| Column | Type | Constraint | Description |
|--------|------|------------|-------------|
| InsuranceConfigId | int | PK, Identity | ID c?u hình |
| InsuranceType | nvarchar(10) | NOT NULL | Lo?i th? |
| Description | nvarchar(200) | NOT NULL | Mô t? |
| DefaultCoveragePercent | decimal(5,2) | NOT NULL | % chi tr? m?c ??nh |
| RequireRegistration | bit | NOT NULL | Yêu c?u ??ng ký n?i KCB |
| IsActive | bit | NOT NULL | ?ang dùng |

---

## ?? QUAN H? CHÍNH (RELATIONSHIPS)

### Core Relationships
```
Departments (1) ----< (N) Staffs
Staffs (1) ----< (N) Appointments [as Doctor]
Patients (1) ----< (N) Appointments
TimeSlots (1) ----< (N) Appointments
Appointments (1) ---< (0..1) Encounters
Patients (1) ----< (N) Encounters
Staffs (1) ----< (N) Encounters [as Doctor]
```

### Encounter Workflow
```
Encounters (1) ----< (N) Orders
Services (1) ----< (N) Orders
Orders (1) ---< (0..1) OrderResults
Encounters (1) ---< (0..1) Invoices
```

### Pharmacy Workflow
```
Medicines (1) ----< (N) MedicineBatches
Medicines (1) ----< (N) PrescriptionItems
Encounters (1) ----< (N) Prescriptions
Staffs (1) ----< (N) Prescriptions [as Doctor]
Prescriptions (1) ----< (N) PrescriptionItems
Prescriptions (1) ----< (N) PharmacyDispenses
PharmacyDispenses (1) ----< (N) DispenseItems
MedicineBatches (1) ----< (N) DispenseItems
MedicineBatches (1) ----< (N) InventoryTransactions
```

### Inpatient Workflow
```
Departments (1) ----< (N) Wards
Wards (1) ----< (N) Beds
Patients (1) ----< (N) Admissions
Beds (1) ----< (N) Admissions
Staffs (1) ----< (N) Admissions [as AttendingDoctor]
Admissions (1) ----< (N) MedicalOrders
Admissions (1) ----< (N) VitalSigns
Admissions (1) ----< (N) Surgeries
Patients (1) ----< (N) Allergies
Patients (1) ----< (N) MedicalHistories
```

### Insurance Workflow
```
Patients (1) ----< (N) InsuranceClaims
Encounters (1) ---< (0..1) InsuranceClaims
Admissions (1) ---< (0..1) InsuranceClaims
InsuranceClaims (1) ----< (N) InsuranceClaimItems
Invoices (N) ---< (0..1) InsuranceClaims
```

---

## ?? ER DIAGRAM (TEXT)

```
???????????????      ???????????????      ???????????????
?  Patients   ???????? Appointments????????   Staffs    ?
???????????????      ???????????????      ???????????????
      ?                     ?                     ?
      ?                     ?                     ?
      ?                     ?                     ?
???????????????      ???????????????             ?
? Encounters  ?      ?  TimeSlots  ?             ?
???????????????      ???????????????             ?
      ?                                           ?
      ?????????????????????????????????????????????
      ?           ?           ?
      ?           ?           ?
??????????????? ??????????????? ???????????????
?   Orders    ? ?Prescriptions? ?   Invoices  ?
??????????????? ??????????????? ???????????????
      ?              ?                ?
      ?              ?                ?
??????????????? ??????????????? ???????????????
?OrderResults ? ?Prescription ? ?Insurance    ?
?             ? ?Items        ? ?Claims       ?
??????????????? ??????????????? ???????????????
```

---

## ?? INDEXES VÀ PERFORMANCE

### Unique Indexes (Business Keys)
- Departments.Code
- TimeSlots.Code
- Medicines.Code
- Prescriptions.Code
- Wards.Code
- Admissions.AdmissionCode
- Surgeries.SurgeryCode
- InsuranceClaims.ClaimCode
- Invoices.InvoiceCode
- (Beds.WardId, Beds.BedNumber)
- OrderResults.OrderId

### Foreign Key Indexes (Auto-created)
T?t c? các foreign keys ??u có index ?? t?i ?u JOIN operations

---

## ?? SECURITY & CONSTRAINTS

### Data Integrity
- ? Primary Keys trên t?t c? các b?ng
- ? Foreign Keys v?i ON DELETE behaviors phù h?p
- ? Unique constraints trên business keys
- ? NOT NULL constraints cho required fields
- ? Default values cho status fields

### Cascade Delete Rules
- **CASCADE:** PrescriptionItems, DispenseItems, InsuranceClaimItems, MedicalOrders, VitalSigns, Allergies, MedicalHistories
- **RESTRICT:** H?u h?t các foreign keys khác (?? tránh xóa nh?m data quan tr?ng)

---

## ?? STORAGE ESTIMATES

Assuming 1000 patients/year, 10 encounters/patient/year:

| Table | Est. Rows/Year | Size/Row | Est. Size |
|-------|----------------|----------|-----------|
| Patients | 1,000 | ~500B | ~500 KB |
| Encounters | 10,000 | ~300B | ~3 MB |
| Orders | 30,000 | ~200B | ~6 MB |
| Prescriptions | 8,000 | ~200B | ~1.6 MB |
| Invoices | 10,000 | ~300B | ~3 MB |
| Admissions | 500 | ~500B | ~250 KB |
| VitalSigns | 10,000 | ~150B | ~1.5 MB |
| **TOTAL** | | | **~16 MB/year** |

---

## ?? MIGRATION HISTORY

1. `InitialCreate` - Core tables
2. `AddAppointmentModule` - Appointments, TimeSlots
3. `AddOrdersAndLab` - Orders, OrderResults
4. `AddInvoices` - Invoice module
5. `AddPharmacyModule` - Pharmacy tables (7 tables)
6. `AddInpatientModule` - Inpatient tables (8 tables)
7. `AddInsuranceModule` - Insurance tables (3 tables)
8. `AddInsuranceModuleFixed` - Decimal precision fixes
9. `SeedPatientsWithInsurance` - Sample data

---

## ?? NOTES

### Design Decisions
1. **Identity columns:** S? d?ng int cho PK (?? cho medium-scale hospital)
2. **Business codes:** Varchar unique indexes cho tra c?u nhanh
3. **DateTime2:** Precision cao h?n datetime
4. **Decimal(18,2):** ?? cho currency (max ~922 tri?u t?)
5. **Enum storage:** Int values (không dùng string ?? ti?t ki?m space)
6. **Soft delete:** S? d?ng IsActive flags thay vì hard delete

### Future Enhancements
- [ ] Add audit trails (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- [ ] Add soft delete patterns (DeletedAt, DeletedBy)
- [ ] Implement temporal tables (history tracking)
- [ ] Add full-text search indexes
- [ ] Partition large tables by date

---

**Generated:** 2026-01-28  
**Database Version:** 1.0  
**EF Core Version:** 10.0.1  
**SQL Server Version:** 2019+
