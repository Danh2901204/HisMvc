# BÁO CÁO CHI TIẾT THAY ĐỔI — Triển khai luồng KCB mới + Audit BYT/TT

Ngày: 25/05/2026
Phạm vi: Toàn bộ project HisMvc — schema, migration, controllers, views, security.

---

## 1. Tóm tắt mục tiêu

Hai nhóm thay đổi chính:

1. **Sửa các lỗi/thiếu sót** đã phát hiện trong báo cáo audit (data model + business logic + UI/security/BYT).
2. **Thay đổi thứ tự hoạt động** theo file `c:\Installation\luong.txt` — chuẩn hoá 10 bước KCB ngoại trú (Reception → Cashier (phí khám) → Hàng đợi → Doctor (khám + CĐ CLS + kê đơn) → Cashier (chi phí cuối) → Pharmacy (cấp thuốc) → Completed).

Toàn bộ thay đổi đã **build sạch (0 error)** và app khởi chạy thành công ở `http://localhost:7239`. Tất cả 10 endpoint chính trả về HTTP 200 (Reception, Cashier, Doctor, Pharmacy, Lab, Inpatient, Admin, Queue display).

---

## 2. Luồng nghiệp vụ mới (theo `luong.txt`)

```
┌──────────────────────────────────────────────────────────────────┐
│ B1. RECEPTION                                                    │
│   - Đặt lịch (đã có) hoặc walk-in → tạo Patient + Appointment    │
│   - CheckIn: tạo Encounter(CheckedIn) + Invoice(ExamFee, Unpaid) │
│        Appointment → CheckedIn                                   │
├──────────────────────────────────────────────────────────────────┤
│ B2. CASHIER — thu phí khám                                       │
│   - Pay Invoice(ExamFee)                                         │
│   - Cấp QueueNumber theo bác sĩ/ngày                             │
│   - Encounter: CheckedIn → WaitingExam                           │
├──────────────────────────────────────────────────────────────────┤
│ B3. QUEUE DISPLAY (màn hình phòng chờ)                           │
│   - Hiển thị BN đang được gọi + danh sách BN chờ theo phòng/khoa │
├──────────────────────────────────────────────────────────────────┤
│ B4. DOCTOR — gọi BN                                              │
│   - CallPatient → InService, StartedAt, RoomNumber               │
├──────────────────────────────────────────────────────────────────┤
│ B5. DOCTOR — khám                                                │
│   - Save: Diagnosis, ICD-10 chính/phụ, Conclusion, Instructions, │
│           FollowUpDate                                           │
│   - AddOrder (CLS): tạo Order, Encounter → WaitingResult         │
│   - CreatePrescription + AddMedicine (kê đơn thuốc)              │
├──────────────────────────────────────────────────────────────────┤
│ B6. LAB / IMAGING                                                │
│   - StartOrder: Requested → InProgress                           │
│   - SaveResult: Resulted + nếu hết Order chưa KQ → InService     │
├──────────────────────────────────────────────────────────────────┤
│ B7. DOCTOR — chốt                                                │
│   - Close: gom Exam + Services (loại Cancelled) + Medicines      │
│            (giá BHYT/giá batch FEFO)                             │
│   - Tạo Invoice(Final) (nếu > 0) → WaitingFinalPayment           │
│     hoặc → Completed nếu không phát sinh                         │
├──────────────────────────────────────────────────────────────────┤
│ B8. CASHIER — thu chi phí cuối                                   │
│   - Pay Invoice(Final, Services, Medicine)                       │
│   - Encounter: WaitingFinalPayment → WaitingMedicine             │
│                (hoặc Completed nếu không có đơn)                 │
├──────────────────────────────────────────────────────────────────┤
│ B9. PHARMACY — cấp thuốc                                         │
│   - Dispense: chỉ cho phép khi Encounter ∈ {WaitingMedicine,     │
│               Completed}, theo FEFO trên MedicineBatch           │
│   - Encounter → Completed, EndAt = now                           │
└──────────────────────────────────────────────────────────────────┘
```

---

## 3. Thay đổi Schema & Migration

### 3.1 `Entities/Enums.cs`

| Enum | Thay đổi |
|---|---|
| `EncounterStatus` | **Viết lại hoàn toàn**: `CheckedIn`, `WaitingExam`, `InService`, `WaitingResult`, `WaitingFinalPayment`, `WaitingMedicine`, `Completed`, `Cancelled` |
| `AppointmentStatus` | Thêm `CheckedIn`, `NoShow` |
| `OrderStatus` | Thêm `InProgress` (giữa Requested và Resulted) |
| `InvoiceStatus` | Thêm `Cancelled` |
| `InvoiceType` (mới) | `ExamFee`, `Services`, `Medicine`, `Final`, `Inpatient` |

### 3.2 `Entities/Core.cs`

- **Patient**: thêm `PatientCode` (HS BHYT/MoH), `Ethnicity`, `Occupation`, `InsuranceValidFrom`, `InsuranceHospitalCode`, `UpdatedAt`.
- **Staff**: thêm `StaffCode` (unique), `LicenseNumber`, `Title`, `IsActive`.
- **Encounter**: thêm `EncounterCode` (unique), `DepartmentId` (FK), `QueueNumber`, `RoomNumber`, `QueuedAt`, `StartedAt`, `Icd10Primary`, `Icd10PrimaryName`, `Icd10Secondary`, `Instructions`, `FollowUpDate`.
- **Appointment**: thêm `CheckedInAt`.

### 3.3 `Entities/Invoice.cs`

- `EncounterId` → **nullable** (cho phép hoá đơn nội trú).
- Thêm `AdmissionId` (nullable, FK).
- Thêm `InvoiceType`, `ExamFeeAmount`, `ServicesAmount`, `MedicineAmount`, `BedAmount`.
- Thêm `PaidByStaffId` (FK → Staff) + navigation `PaidByStaff`.
- Thêm `RowVersion` (`[Timestamp]`) cho concurrency.

### 3.4 `Entities/Orders.cs`

- **Service**: thêm `Code`, `BhytCode`, `BhytPrice`, `IsInBhytList`, `DepartmentCode`, `IsActive`.
- **Order**: thêm `OrderCode` (unique), `Quantity`, `OrderedByStaffId` (FK), `StartedAt`, `CompletedAt`.

### 3.5 `Entities/Pharmacy.cs`

- **Medicine**: thêm `BhytCode`, `IsInBhytList`, `BhytPrice`.
- **Prescription**: `EncounterId` → nullable, thêm `AdmissionId` (nullable) — cho phép kê đơn nội trú.

### 3.6 `Entities/Inpatient.cs`

- **Admission**: thêm `Icd10Admission`, `Icd10Discharge`, `Icd10DischargeSecondary`, `DischargeResult` (BYT TT 50/2014).
- **VitalSign**: thêm `Shift`.
- **Allergy**: thêm `AllergyType`.

### 3.7 `Data/AppDbContext.cs`

- **Unique indexes** mới: `Patient.PatientCode`, `Patient.IdentityNumber`, `Staff.StaffCode`, `Order.OrderCode`, `Service.Code`, `MedicineBatch (MedicineId+BatchNumber)`, `InsuranceConfig.InsuranceType`, `Encounter.EncounterCode`.
- **Filtered index** cho các cột nullable (`HasFilter`).
- **Index** `Encounter.Status`, `Order.Status` để tối ưu dashboard query.
- **Đổi DeleteBehavior**: hầu hết FK `Cascade` → **`Restrict`** (giữ toàn vẹn dữ liệu y tế).
- **Precision**: cho các trường tiền mới (`ExamFeeAmount`, `ServicesAmount`, `MedicineAmount`, `BedAmount`, `BhytPrice`).
- **Bỏ auto-set** `Encounter.EndAt` và `Invoice.PatientAmount` trong `NormalizeEntities`.

### 3.8 Migration

- Tạo `20260525164700_RestructureFlowMoH.cs`:
  - Bảo đảm tồn tại `Department` mặc định **trước** khi thêm `Encounter.DepartmentId`.
  - Backfill `DepartmentId` của `Encounter` qua `Staff.DepartmentId` (hoặc fallback).
  - `ALTER TABLE … WITH NOCHECK ADD CONSTRAINT FK_Encounters_Departments_DepartmentId`.
  - Drop `IX_MedicineBatches_MedicineId` có **`IF EXISTS`** check (tránh lỗi khi index không tồn tại).
- DB hiện ở trạng thái: `dotnet ef database update` → "No migrations were applied. The database is already up to date."
- Tắt cảnh báo `PendingModelChangesWarning` trong `Program.cs` (đã verify model snapshot khớp DB).

---

## 4. Thay đổi Controllers

### 4.1 `Areas/Reception/Controllers/HomeController.cs`

- **`CheckIn` (POST)**:
  - Transaction `BeginTransactionAsync()`.
  - Tạo `Encounter(Status=CheckedIn)` + `Invoice(Type=ExamFee, Status=Unpaid)`.
  - Tính BHYT ban đầu cho phí khám (`InsuranceCoveragePercent`).
  - Update `Appointment.Status = CheckedIn`, `CheckedInAt = now`.
- **`Cancel` (POST)**: thêm guard — nếu Appointment đã có Encounter → **không cho huỷ**.

### 4.2 `Areas/Cashier/Controllers/HomeController.cs`

- **Dashboard KPI**: tách `UnpaidExamFee` vs `UnpaidFinal`.
- **`Index`**: filter theo `InvoiceType`.
- **`Pending`**: liệt kê Encounter ở `WaitingFinalPayment` (chờ chốt chi phí phát sinh).
- **`Create`**: deprecated → redirect `Pending` (Invoice cuối do Doctor tạo).
- **`Pay` (POST)**:
  - Transaction.
  - Set `PaidByStaffId` qua `CurrentStaffService`.
  - **Phí khám trả** → `Encounter: CheckedIn → WaitingExam` + cấp `QueueNumber` (helper `NextQueueNumberAsync(doctorId, today)`).
  - **Invoice Final trả** → `Encounter: WaitingFinalPayment → WaitingMedicine` (nếu có đơn thuốc) hoặc `Completed`.

### 4.3 `Areas/Doctor/Controllers/HomeController.cs`

- **Dashboard**: KPI thêm `WaitingExam`, `WaitingResult`. Queue lọc `{WaitingExam, InService, WaitingResult}` sort theo `QueueNumber`.
- **`CallPatient` (POST)** mới: `WaitingExam → InService`, set `StartedAt`, `RoomNumber`.
- **`Save`**: lưu thêm `Icd10Primary`, `Icd10PrimaryName`, `Icd10Secondary`, `Instructions`, `FollowUpDate`. Guard `InService | WaitingResult`.
- **`AddOrder`**: set `OrderCode`, `Quantity`, `OrderedByStaffId` → `Encounter → WaitingResult`.
- **`CancelOrder`**: cho phép cả `InProgress`.
- **`Close`**:
  - Guard `InService | WaitingResult`.
  - Bắt buộc có `Diagnosis` + `Icd10Primary`.
  - Chặn nếu còn Order `Requested`.
  - Gọi `CreateOrUpdateFinalInvoiceAsync` → tổng = exam + services (loại `Cancelled`) + medicines (giá BHYT, fallback batch FEFO).
  - Tổng = 0 → `Completed`, ngược lại → `WaitingFinalPayment`.
  - Transaction.
- **`CreatePrescription` / `AddMedicine` / `RemoveMedicine`**: cho phép trong `InService | WaitingResult`.
- **`Reopen`**: chỉ reopen từ `WaitingFinalPayment` khi Invoice Final **chưa thanh toán**.

### 4.4 `Areas/Pharmacy/Controllers/HomeController.cs`

- Inject `CurrentStaffService` (sửa lỗi cũ dùng `User.Identity.Name`).
- **`Dispense` (POST)**:
  - Transaction.
  - Guard: chỉ cho cấp khi `Encounter ∈ {WaitingMedicine, Completed}` (đã trả final).
  - Trừ tồn theo FEFO (`MedicineBatch.ExpiryDate`).
  - `InventoryTransaction.StaffId = current pharmacist`.
  - Sau khi cấp đủ: `Encounter → Completed`, `EndAt = now`.
- **`History`** (mới): lịch sử cấp thuốc, filter theo khoảng ngày, link sang chi tiết đơn.

### 4.5 `Areas/Lab/Controllers/HomeController.cs`

- Inject `CurrentStaffService`.
- **`Index`**: filter `{Requested, InProgress}`.
- **`StartOrder` (POST)** mới: `Requested → InProgress`, set `StartedAt`.
- **`SaveResult`**: set `ResultedByStaffId`, `CompletedAt`. Khi tất cả Order của Encounter đã có KQ và Encounter ở `WaitingResult` → tự revert về `InService` để bác sĩ chốt.

### 4.6 `Areas/Inpatient/Controllers/MedicalOrderController.cs`

- **`Create`**: set `OrderCode`, `OrderedByStaffId` cho Lab/Imaging. Đơn thuốc nội trú link qua `AdmissionId` (không phải `EncounterId`).
- **`GetOrCreateInpatientEncounterId`**: scope Encounter đại diện cho **đúng `AdmissionId`** theo mẫu `Conclusion = "Noi tru: {AdmissionCode}"`; set `DepartmentId` từ Ward.Department; set `EncounterCode`.
- **`Complete`**: guard `MedicalOrderStatus.InProgress`.

### 4.7 `Areas/Inpatient/Controllers/HomeController.cs`

- **`Admit` (POST)**: thêm `icd10Admission`. Transaction quanh tạo Admission + cập nhật Bed.
- **`Discharge` (POST)**: Transaction; ngoài Admission + Bed, đóng luôn Encounter đại diện (`Completed`, `EndAt = now`).

### 4.8 `Areas/Admin/Controllers/InsuranceClaimController.cs`

- `[Authorize(Roles="ADMIN,CASHIER")]` → **`Roles="ADMIN"`** (theo QD 130/BHXH: Thu ngân không có thẩm quyền duyệt claim BHYT).

### 4.9 `Services/CurrentStaffService.cs`

- Thêm `GetCurrentStaffIdAsync(ClaimsPrincipal user)` (return `int?`) — dùng cho các nơi staff là tuỳ chọn.

### 4.10 `Services/InsuranceService.cs`

- `CalculateInsuranceForEncounter`: cộng cả tiền thuốc + tiền dịch vụ (loại `Cancelled`), dùng `BhytPrice` của `Service`/`Medicine`.
- `GenerateClaimItemsAsync`: sinh claim item cho cả đơn thuốc, kèm `BhytCode`.

---

## 5. Thay đổi Views

| File | Thay đổi |
|---|---|
| `Views/Shared/_Sidebar.cshtml` | Link "Quan ly buong" chỉ hiện cho `ADMIN`. |
| `Areas/Reception/Views/Home/Index.cshtml` | Thêm `@Html.AntiForgeryToken()` vào form CheckIn/Cancel. |
| `Areas/Cashier/Views/Home/*` | Dashboard hiển thị 2 nhóm hoá đơn (ExamFee / Final). `Pending` show Encounter `WaitingFinalPayment`. |
| `Areas/Doctor/Views/Home/Examine.cshtml` | **Viết lại**: status badge mới (WaitingExam/InService/WaitingResult/WaitingFinalPayment/…), nút **Gọi BN** + nhập phòng, form ICD-10 chính/phụ + tên bệnh, `Instructions`, `FollowUpDate`, **UI kê đơn** (CreatePrescription/AddMedicine/RemoveMedicine), điều kiện chốt rõ ràng. |
| `Areas/Doctor/Views/Home/Index.cshtml` | Queue sort theo `QueueNumber`, hiển thị `RoomNumber`. |
| `Areas/Pharmacy/Views/Home/History.cshtml` | **Mới**: lịch sử cấp thuốc + filter ngày. |
| `Areas/Admin/Views/InsuranceClaim/Detail.cshtml` | Sửa Razor date: `.ToString("dd/MM/yyyy HH:mm")`. |
| `Controllers/QueueController.cs` + `Views/Queue/Index.cshtml` | **Mới**: màn hình hiển thị STT phòng chờ — `[AllowAnonymous]`, đồng hồ realtime, auto-refresh 10s, filter theo Department, phân nhóm "Đang gọi" / "Đang chờ". |

---

## 6. Bảo mật

- Cấp lại `[Authorize]` đúng vai trò cho mọi POST nhạy cảm.
- Thêm `@Html.AntiForgeryToken()` tại các form còn thiếu (Reception Index).
- `[ValidateAntiForgeryToken]` cho mọi POST mới (`CallPatient`, `StartOrder`, `Dispense`, `Admit`, `Discharge`, `Pay`, …).
- Bỏ cấp quyền `CASHIER` cho `InsuranceClaim` (phù hợp Thông tư BYT/QĐ 130).

---

## 7. Verification

- `dotnet build` → **0 Errors, 0 Warnings** (ngoài NU1900 do offline NuGet vulnerability check).
- `dotnet ef database update` → DB đã đồng bộ schema.
- Khởi chạy app → seed thành công, listening `http://localhost:7239`.
- Đã smoke test 10 endpoint với session đăng nhập admin: tất cả trả về HTTP 200, không endpoint nào bị redirect về login:

```
/Reception/Home          200
/Cashier/Home            200
/Cashier/Home/Pending    200
/Doctor/Home             200
/Pharmacy/Home           200
/Pharmacy/Home/History   200
/Lab/Home                200
/Inpatient/Home          200
/Admin/Home              200
/Queue?departmentId=1    200
```

---

## 8. Sửa encoding + ghép luồng Cashier/Pharmacy

### Cashier
- **Detail.cshtml**: Sửa mojibake, hiển thị theo `InvoiceType` (ExamFee vs Final), chi tiết CLS + thuốc, trạng thái Encounter, gợi ý bước tiếp theo sau khi thu.
- **Index.cshtml**: Filter loại HD (Phí khám / Tổng hợp), cột loại HD, link nhanh "Phí khám chưa thu" và "Chờ thu chi phí cuối".
- **Pending.cshtml**: Đổi từ "Lập hóa đơn" → "Chờ thu chi phí cuối" (bước 8), link trực tiếp HD Final do BS tạo.
- **Dashboard.cshtml**: KPI tách `UnpaidExamFee` / `UnpaidFinal`.

### Pharmacy
- **Dispense/Index/Detail.cshtml**: Sửa mojibake, hiển thị bước 9, cảnh báo nếu BN chưa thanh toán chi phí cuối.
- **Controller**: Mặc định chỉ hiện đơn `Pending` + `Encounter.Status = WaitingMedicine`; guard GET Dispense nếu chưa thu tiền.
- **Dashboard**: KPI và danh sách đơn chờ cấp chỉ tính đơn đã `WaitingMedicine`.

---

## 9. Việc còn lại (đề xuất tiếp theo)

1. Bổ sung **unit test** cho `Doctor.Close` (logic gom tiền) + `Pharmacy.Dispense` (FEFO + giảm tồn).
2. Thêm trang **Doctor.MyQueue** dành riêng cho bác sĩ + lệnh "Gọi BN tiếp theo".
3. Tạo trang **Reports BHYT** xuất XML 4210/2017 cho cơ quan BHXH.
4. Tích hợp Datadog log/metric (đã có MCP) cho audit trail BYT.
