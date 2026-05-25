# Cau truc project — Mo hinh MVC

## Tong quan

```
HisMvc/
├── Controllers/          # MVC — trang chung (Home, Queue, Account)
├── Views/                # MVC — giao dien chung
├── Models/               # MVC — ViewModel + DTO dung chung
├── Entities/             # Du lieu (Entity Framework) — KHONG phai ViewModel
├── Data/                 # DbContext, Seed, migration
├── Services/             # Logic nghiep vu (Controller KHONG viet logic o day)
│   ├── Workflow/         # Luong KCB ngoai tru — 10 buoc
│   │   ├── ReceptionWorkflowStep.cs   # Buoc 1: Check-in
│   │   ├── CashierWorkflowStep.cs     # Buoc 2, 8: Thu phi kham / thanh toan cuoi
│   │   ├── DoctorWorkflowStep.cs      # Buoc 4, 7: Kham, ket luan
│   │   ├── LabWorkflowStep.cs         # Buoc 6: Ket qua CLS
│   │   ├── PharmacyWorkflowStep.cs    # Buoc 9: Cap thuoc
│   │   ├── BillingHelper.cs
│   │   └── WorkflowDbHelper.cs
│   ├── OutpatientWorkflowService.cs   # Facade — Controller goi class nay
│   ├── InsuranceService.cs
│   └── CurrentStaffService.cs
├── Areas/                # MVC theo phong ban
│   ├── Reception/   Models/ + Services/ReceptionViewService.cs
│   ├── Cashier/     Models/ + Services/CashierViewService.cs
│   ├── Doctor/      Models/ + Services/DoctorViewService.cs
│   ├── Pharmacy/    Models/ + Services/PharmacyViewService.cs
│   ├── Lab/         Models/ + Services/LabViewService.cs
│   ├── Inpatient/   Models/ + Services/InpatientViewService.cs
│   └── Admin/       Models/ + Services/AdminViewService.cs
└── wwwroot/              # CSS, JS, hinh anh
```

## ViewService da hoan thanh

| Area | ViewService | Controllers dung ViewService (GET) |
|------|-------------|-------------------------------------|
| Reception | `ReceptionViewService` | HomeController |
| Cashier | `CashierViewService` | HomeController |
| Doctor | `DoctorViewService` | HomeController |
| Pharmacy | `PharmacyViewService` | HomeController, MedicineController |
| Lab | `LabViewService` | HomeController |
| Admin | `AdminViewService` | Home, Patient, InsuranceClaim, Department, Service, TimeSlot, InsuranceConfig, Staff, User |
| Inpatient | `InpatientViewService` | Home, MedicalOrder, Surgery, Ward |

Dang ky DI trong `Program.cs`:

```csharp
builder.Services.AddScoped<CashierViewService>();
builder.Services.AddScoped<DoctorViewService>();
builder.Services.AddScoped<PharmacyViewService>();
builder.Services.AddScoped<ReceptionViewService>();
builder.Services.AddScoped<LabViewService>();
builder.Services.AddScoped<AdminViewService>();
builder.Services.AddScoped<InpatientViewService>();
builder.Services.AddScoped<OutpatientWorkflowService>();
```

## Phan vai tung lop (MVC)

| Lop | Thu muc | Lam gi | Khong lam gi |
|-----|---------|--------|--------------|
| **Model** | `Entities/`, `Models/`, `Areas/*/Models/` | Entity DB, ViewModel hien thi | HTML, HTTP |
| **View** | `Views/`, `Areas/*/Views/` | HTML, form, bang du lieu | Truy van DB, doi trang thai |
| **Controller** | `Controllers/`, `Areas/*/Controllers/` | Nhan URL, goi Service, chon View | Logic tinh tien, doi EncounterStatus |

## Luong xu ly 1 request

```
Browser → Controller (GET/POST)
              ↓
         ViewService (doc du lieu)     hoac     WorkflowService (ghi/thay doi)
              ↓                                      ↓
         ViewModel                                 WorkflowResult
              ↓                                      ↓
         View (.cshtml)                         TempData + Redirect
```

## Luong KCB ngoai tru (10 buoc)

| Buoc | Vai tro | EncounterStatus |
|------|---------|-----------------|
| 1 | Reception Check-in | CheckedIn |
| 2 | Cashier thu phi kham | WaitingForExam |
| 3 | Doctor goi BN | InExam |
| 4 | Doctor kham + chi dinh CLS | InExam |
| 5 | (Queue display) | — |
| 6 | Lab nhap ket qua | AwaitingResults / ResultsReady |
| 7 | Doctor ket luan | WaitingFinalPayment / Completed |
| 8 | Cashier thanh toan cuoi | WaitingDispense |
| 9 | Pharmacy cap thuoc | Completed |
| 10 | Hoan tat | Completed |

Chi tiet: `LUONG_KCB_SIMPLE.md`, `CHANGELOG_LUONG_BYT.md`

## Vi du: Thu ngan xem hoa don

1. **Controller** `Cashier/HomeController.Detail(id)` — goi ViewService
2. **ViewService** `CashierViewService.GetInvoiceDetailAsync(id)` — doc DB, tao `InvoiceDetailViewModel`
3. **View** `Areas/Cashier/Views/Home/Detail.cshtml` — `@model InvoiceDetailViewModel`

## Vi du: Thu tien (POST)

1. **Controller** `Pay(id)` — goi `_workflow.PayInvoiceAsync(...)`
2. **Workflow** `CashierWorkflowStep.PayInvoiceAsync` — doi trang thai, transaction
3. **Controller** — `TempData["Success"]` + `RedirectToAction`

## Vi du: Admin CRUD (form co prefix)

1. **GET** — `AdminViewService.GetStaffFormAsync()` tra `StaffFormViewModel`
2. **View** — `@model StaffFormViewModel`, field `asp-for="Staff.FullName"`
3. **POST** — `[Bind(Prefix = "Staff")] Staff model`

## Quy tac khi them tinh nang

1. Entity moi → `Entities/`
2. Man hinh moi → `Areas/{Ten}/Views/` + ViewModel trong `Areas/{Ten}/Models/`
3. Doc du lieu phuc tap → `Areas/{Ten}/Services/{Ten}ViewService.cs`
4. Thay doi luong KCB → `Services/Workflow/*WorkflowStep.cs`

Xem them: `LUONG_KCB_SIMPLE.md`
