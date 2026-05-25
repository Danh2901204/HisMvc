# Can chinh schema theo Book1.xlsx va Thong tu Bo Y Te VN

## Nguon tham chieu

- **Book1.xlsx** — export day du 35 bang nghiep vu (+ bang Identity ASP.NET)
- **Server:** `localhost` (SQL Server) / **Database:** `HIS_MVC_DB`
- **Connection:** `appsettings.json` -> `ConnectionStrings:Default`

## Quy chuan ap dung

| Quy chuan | Pham vi |
|-----------|---------|
| TT 56/2017/TT-BYT, TT 32/2023/TT-BYT | Ho so benh an (kham, nhap vien, y lenh, phieu mo) |
| TT 50/2014/TT-BYT | Phan loai phau thuat, thu thuat |
| TT 22/2023/TT-BYT | Gia kham chua benh BHYT |
| QD 4210/QD-BYT 2017 | Chuan du lieu XML giam dinh BHYT |
| QD 130/QD-BHXH | Chuan ho so giam dinh XML (XML 1, XML 2...) |
| Luat BHYT (95/2008, 46/2014) | Ty le chi tra (100% / 95% / 80% / 60%) |

## Migration `AlignWithBook1Schema` (20260525145027)

Da apply tren `HIS_MVC_DB`:

- Xoa shadow FK thua: `DischargedByStaffStaffId`, `ExecutedByStaffStaffId`, `RecordedByStaffStaffId`, `ApprovedByStaffStaffId`
- FK chuan: `DischargedBy`, `ExecutedBy`, `RecordedBy`, `ApprovedBy`, `InventoryTransactions.StaffId`
- Default NOT NULL: `Patients.InsuranceCoveragePercent=0`, `Invoices.HasInsurance/InsuranceAmount/PatientAmount=0`, `PrescriptionItems.Duration=1`

```powershell
cd HisMvc
dotnet ef database update
```

## Danh sach 35 bang Book1.xlsx — phu hop voi entity

### Core (Reception / Doctor)

| Bang | Module | Map UI |
|------|--------|--------|
| `Patients` | Ho so BN | Admin > Benh nhan + Reception |
| `Staffs`, `Departments` | Nhan vien, khoa | Admin |
| `TimeSlots`, `Appointments` | Lich hen | Reception |
| `Encounters` | Luot kham | Reception / Doctor |
| `Services`, `Orders`, `OrderResults` | Chi dinh CLS | Doctor / Lab |
| `Invoices` | Hoa don | Cashier |

### Pharmacy

| Bang | Module | Map UI |
|------|--------|--------|
| `Medicines`, `MedicineBatches` | Thuoc, lo | Pharmacy |
| `Prescriptions`, `PrescriptionItems` | Don thuoc | Doctor (tao) / Pharmacy (cap) |
| `PharmacyDispenses`, `DispenseItems` | Cap phat | Pharmacy |
| `InventoryTransactions` | Xuat / nhap kho | Pharmacy |

### Inpatient — Hoso benh an noi tru (TT 56/2017)

| Bang | Module | Map UI |
|------|--------|--------|
| `Wards`, `Beds` | Buong, giuong | Inpatient > Ward |
| `Admissions` | Ho so nhap vien | Inpatient > Home |
| **`MedicalOrders`** | **Y lenh — Mau 01/BV-01** | **Inpatient > MedicalOrder** |
| **`Surgeries`** | **Phau thuat — Mau 03/BV-01** | **Inpatient > Surgery** |
| `VitalSigns` | Sinh hieu | Inpatient > AddVitalSign |
| `Allergies`, `MedicalHistories` | EMR | Admin > Patient > Details (canh bao tu dong khi kham/y lenh) |

### BHYT — Giam dinh (TT 22/2023 + QD 4210 + QD 130)

| Bang | Module | Map UI |
|------|--------|--------|
| `InsuranceConfigs` | Cau hinh ty le | **Admin > Cau hinh BHYT** |
| **`InsuranceClaims`** | **Ho so giam dinh** | **Admin > Giam dinh BHYT** |
| **`InsuranceClaimItems`** | **Chi tiet DVKT (XML 2)** | Trong man Giam dinh |

## Luong nghiep vu chuan BYT

### 1. Kham ngoai tru
```
Le tan -> Tao lich hen / check-in
  -> Bac si "Kham" (canh bao di ung/tien su tu dong)
    -> Chi dinh CLS (-> Lab nhap KQ)
    -> Ke don thuoc
  -> "Chot luot kham" (sinh Invoice)
-> Thu ngan thu tien (kem tinh BHYT neu co)
  -> Tu dong tao InsuranceClaim + Items
-> Admin > Giam dinh BHYT: Nop -> Duyet/Tu choi -> Xuat XML
```

### 2. Noi tru
```
Bac si nhap vien (chon giuong + ly do)
-> Y lenh hang ngay (Thuoc/CLS/Thu thuat/Cham soc/Dinh duong)
  - Voi thuoc: auto tao Prescription
  - Voi CLS: auto tao Order de Lab thuc hien
-> Sinh hieu (theo doi 3 lan/ngay)
-> Phau thuat (Lap phieu -> Bat dau -> Bien ban + Y lenh sau mo)
-> Xuat vien (tom tat dieu tri + huong dan)
```

### 3. Giam dinh BHYT (QD 130/QD-BHXH)
```
Cashier tao hoa don co BHYT
  -> InsuranceClaim status = Pending
Admin > Giam dinh BHYT
  -> Submit (chuyen Submitted, sinh XML 1+2)
  -> Co quan BHYT Approve / Reject / PartiallyApproved
  -> Tai XML giam dinh chuan 130/QD-BHXH
```

## Tinh nang da bo sung (tuan thu thong tu)

1. **Y lenh noi tru** — `MedicalOrder` voi day du loai (Thuoc/Lab/Imaging/Thu thuat/Cham soc/Dinh duong),
   ghi nguoi ra y lenh + nguoi thuc hien + thoi gian
2. **Phieu phau thuat / thu thuat** — `Surgery` voi Phau thuat vien, Gay me, Phong mo, Bien ban PT,
   Y lenh sau mo, Bien chung
3. **Giam dinh BHYT** — `InsuranceClaim` voi `InsuranceClaimItems`, XML giam dinh chuan 130/QD-BHXH
   (XML 1 thong tin chung + XML 2 chi dinh DVKT)
4. **Canh bao di ung / tien su benh** — Hien thi tu dong khi bac si kham hoac vao ho so noi tru
5. **Lich su kham truoc do** — Bac si thay 5 luot kham gan nhat cua BN
6. **Cau hinh BHYT** — Quan ly ty le mac dinh theo loai the (QN, KC, GD, HT...)

## Loi luu thuong gap da xu ly

1. **InsuranceCoveragePercent NULL** — default 0 + `NormalizeEntities`
2. **Encounter.EndAt NULL** — set khi check-in
3. **StaffId sai** — `CurrentStaffService` lay tu `AspNetUsers.StaffId`
4. **VitalSigns.RecordedBy = 0** — lay staff tu tai khoan
5. **Shadow FK `*StaffStaffId`** — da drop, EF map cot chuan

## Lien ket tai khoan — nhan vien

Trong **Admin > Tai khoan**, moi user can co **StaffId**:

| Tai khoan | Role | Quyen truy cap |
|-----------|------|----------------|
| `admin@his.local` | ADMIN | Tat ca |
| `doctor@his.local` | DOCTOR | Doctor, Inpatient (Y lenh + PT) |
| `reception@his.local` | RECEPTION | Tiep nhan, lich hen |
| `lab@his.local` | LAB_TECH | Lab |
| `pharmacist@his.local` | PHARMACIST | Pharmacy |
| `cashier@his.local` | CASHIER | Thu ngan + Giam dinh BHYT |

Mat khau mac dinh: `123456`

## Cap nhat DB

```powershell
cd HisMvc
dotnet ef database update
dotnet run
```
