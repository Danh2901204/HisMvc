# Huong dan doc code — Luong KCB ngoai tru

## Mo hinh MVC

| Thanh phan | Thu muc | Vai tro |
|------------|---------|---------|
| **Model** | `Entities/`, `Models/`, `Areas/*/Models/` | Du lieu DB + ViewModel |
| **View** | `Views/`, `Areas/*/Views/` | HTML Razor |
| **Controller** | `Areas/*/Controllers/` | Nhan request, goi Service, tra View |

Chi tiet cau truc: **`PROJECT_STRUCTURE.md`**

---

## Luong KCB — doc o dau?

Logic nam trong **`Services/Workflow/`** — moi buoc 1 file:

| File | Buoc |
|------|------|
| `ReceptionWorkflowStep.cs` | 1 — Check-in |
| `CashierWorkflowStep.cs` | 2, 8 — Thu tien |
| `DoctorWorkflowStep.cs` | 4, 7 — Goi BN, chot kham |
| `LabWorkflowStep.cs` | 6 — Luu ket qua CLS |
| `PharmacyWorkflowStep.cs` | 9 — Cap thuoc |

Controller goi **`OutpatientWorkflowService`** (facade), khong goi truc tiep tung Step.

---

## Mau Controller (POST — thay doi du lieu)

```csharp
var result = await _workflow.PayInvoiceAsync(id, userName, staffId);
TempData[result.Success ? "Success" : "Error"] = result.Message;
return RedirectToAction(...);
```

## Mau Controller (GET — hien thi man hinh)

```csharp
// Cashier — dung ViewService + ViewModel
var model = await _views.GetInvoiceDetailAsync(id);
return View(model);
```

View: `@model InvoiceDetailViewModel` — khong dung ViewBag neu co the.

---

## Area Cashier (mau MVC day du)

```
Areas/Cashier/
├── Controllers/HomeController.cs     ← mong, ~70 dong
├── Models/CashierViewModels.cs       ← ViewModel
├── Services/CashierViewService.cs    ← doc DB cho View
└── Views/Home/*.cshtml               ← HTML
```

Cac Area khac (Doctor, Pharmacy, ...) co the lam tuong tu khi can.

---

## Sua loi thuong gap

| Van de | Xem file |
|--------|----------|
| BN khong cap STT | `CashierWorkflowStep.AfterExamFeePaidAsync` |
| Chot kham sai trang thai | `DoctorWorkflowStep.CloseEncounterAsync` |
| Khong cap thuoc | `PharmacyWorkflowStep.CanDispenseMedicine` |
