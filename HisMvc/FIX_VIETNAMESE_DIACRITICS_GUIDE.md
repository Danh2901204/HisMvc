# ? H??NG D?N S?A TI?NG VI?T CÓ D?U ? KHÔNG D?U

## ?? V?N ??: 
Encoding UTF-8 trong C# và SQL Server có th? gây l?i khi hi?n th? ti?ng Vi?t có d?u.

## ? GI?I PHÁP:
S? d?ng ti?ng Vi?t KHÔNG D?U trong toàn b? project.

---

## ?? B?NG CHUY?N ??I NHANH

### T? th??ng dùng:
```
Có d?u          ? Không d?u
----------------------------------------
Thông tin       ? Thong tin
B?nh nhân       ? Benh nhan
Khoa            ? Khoa (gi? nguyên)
L?ch h?n        ? Lich hen
??ng nh?p       ? Dang nhap
??ng xu?t       ? Dang xuat
Thêm m?i        ? Them moi
C?p nh?t        ? Cap nhat
Xóa             ? Xoa
Tìm ki?m        ? Tim kiem
Khám b?nh       ? Kham benh
Ch? ??nh        ? Chi dinh
Xét nghi?m      ? Xet nghiem
K?t qu?         ? Ket qua
D?ch v?         ? Dich vu
Thu?c           ? Thuoc
??n thu?c       ? Don thuoc
C?p phát        ? Cap phat
Nh?p vi?n       ? Nhap vien
Xu?t vi?n       ? Xuat vien
Gi??ng          ? Giuong
Bu?ng b?nh      ? Buong benh
Hóa ??n         ? Hoa don
Thanh toán      ? Thanh toan
B?o hi?m        ? Bao hiem
Qu?n lý         ? Quan ly
```

---

## ?? CÁC FILE C?N S?A (?U TIÊN)

### ? ?Ã S?A:
- [x] `Views/Shared/_Layout.cshtml` - Menu navigation
- [x] `Data/SeedData.cs` - Departments, Staffs (m?t ph?n)

### ?? C?N S?A (QUAN TR?NG):

#### 1. **Controllers** (TempData messages)
```
HisMvc/Areas/Admin/Controllers/*.cs
HisMvc/Areas/Reception/Controllers/*.cs
HisMvc/Areas/Doctor/Controllers/*.cs
HisMvc/Areas/Lab/Controllers/*.cs
HisMvc/Areas/Pharmacy/Controllers/*.cs
HisMvc/Areas/Inpatient/Controllers/*.cs
HisMvc/Areas/Cashier/Controllers/*.cs
```

**Ví d? s?a:**
```csharp
// TR??C:
TempData["Success"] = "Thêm thu?c thành công!";

// SAU:
TempData["Success"] = "Them thuoc thanh cong!";
```

#### 2. **Views** (Labels, Headers, Buttons)
```
HisMvc/Areas/*/Views/**/*.cshtml
```

**Ví d? s?a:**
```html
<!-- TR??C: -->
<h2>Thông Tin B?nh Nhân</h2>
<label>H? tên <span class="text-danger">*</span></label>

<!-- SAU: -->
<h2>Thong Tin Benh Nhan</h2>
<label>Ho ten <span class="text-danger">*</span></label>
```

#### 3. **SeedData.cs** (Sample data)
?ã s?a m?t ph?n, c?n s?a ti?p:
- Medicines descriptions
- Patient names
- Ward names
- Etc.

---

## ?? CÁCH S?A NHANH B?NG VS CODE

### Ph??ng pháp 1: Find & Replace (Ctrl+Shift+H)
1. M? Find & Replace trong toàn b? workspace
2. Tìm t? có d?u, replace b?ng không d?u
3. Nh?n "Replace All"

**L?u ý:** C?n th?n v?i các bi?n, method names không nên ??i!

### Ph??ng pháp 2: Extension
Cài ??t extension: **"Vietnamese - Telex"** ho?c **"Remove Diacritics"**

---

## ?? CHECKLIST CHI TI?T

### SEEDDATA.CS
- [x] Department names
- [x] Staff names
- [x] Service names
- [ ] Medicine descriptions
- [ ] Patient sample data
- [ ] Ward names

### CONTROLLERS
- [ ] Admin controllers (6 files)
- [ ] Reception controller
- [ ] Doctor controller
- [ ] Lab controller
- [ ] Pharmacy controllers (2 files)
- [ ] Inpatient controllers (2 files)
- [ ] Cashier controller

### VIEWS
#### Admin Area (15+ views)
- [ ] Home/Index
- [ ] User/Index, Create, Edit
- [ ] Department/Index, Create, Edit
- [ ] Staff/Index, Create, Edit
- [ ] Service/Index, Create, Edit
- [ ] TimeSlot/Index, Create, Edit

#### Reception Area (5 views)
- [ ] Home/Index
- [ ] Home/Create
- [ ] Home/Patients
- [ ] Home/CreatePatient
- [ ] Home/EditPatient

#### Doctor Area (2 views)
- [ ] Home/Index
- [ ] Home/Examine

#### Lab Area (3 views)
- [ ] Home/Index
- [ ] Home/Result
- [ ] Home/History

#### Pharmacy Area (9 views)
- [ ] Home/Index, Detail, Dispense
- [ ] Medicine/Index, Create, Edit, Stock, AddBatch, LowStock

#### Inpatient Area (10 views)
- [ ] Home/Index, Detail, Admit, Discharge, AddVitalSign
- [ ] Ward/Index, Create, Edit, BedMap, AddBed

#### Cashier Area (4 views)
- [ ] Home/Index, Pending, Create, Detail

---

## ??? SCRIPT T? ??NG (Nâng cao)

N?u mu?n t? ??ng hóa hoàn toàn, có th? dùng script PowerShell ho?c Python.

### Script PowerShell (?ã cung c?p):
```powershell
.\convert-vietnamese-to-no-diacritics.ps1
```

**L?u ý:** C?n test k? tr??c khi ch?y trên toàn b? project!

---

## ?? L?U Ý QUAN TR?NG

### ? KHÔNG S?A:
1. **Tên bi?n, tên method, tên class:** Gi? nguyên
2. **Comments trong code:** Có th? gi? có d?u
3. **Namespace, using statements:** Không ch?m vào
4. **Database column names:** Không ??i
5. **Enum names:** Không ??i

### ? CH? S?A:
1. **UI text** (labels, headers, buttons)
2. **TempData messages**
3. **Sample data** (names, descriptions)
4. **Placeholder text**
5. **Help text, instructions**

---

## ?? ?U TIÊN S?A THEO TH? T?

### M?c 1 - CRITICAL (Ph?i s?a ngay):
1. ? _Layout.cshtml (menu)
2. ? SeedData.cs (sample data)
3. Controllers TempData messages

### M?c 2 - HIGH (Nên s?a):
4. Views hi?n th? chính (Index, Create, Edit)
5. Form labels & placeholders

### M?c 3 - MEDIUM (Có th? s?a sau):
6. Detail views
7. Help text
8. Tooltips

### M?c 4 - LOW (Tùy ch?n):
9. Comments trong code
10. Documentation

---

## ?? CÁCH TEST SAU KHI S?A

1. **Build project:**
```bash
dotnet build
```

2. **Ch?y app:**
```bash
dotnet run
```

3. **Test t?ng module:**
- Login v?i các roles khác nhau
- Th? t?o m?i, s?a, xóa
- Ki?m tra hi?n th? text
- Ki?m tra TempData messages

4. **Ki?m tra database:**
- Xem sample data có load ?úng không
- Ki?m tra encoding trong SQL Server

---

## ?? H? TR?

N?u g?p l?i encoding sau khi s?a:

1. **Check UTF-8 BOM:**
   - M? file b?ng Notepad++
   - Menu: Encoding ? UTF-8 (without BOM)

2. **SQL Server Collation:**
   - Nên dùng: `Latin1_General_CI_AS`
   - Ho?c: `SQL_Latin1_General_CP1_CI_AS`

3. **appsettings.json:**
   - ??m b?o connection string ?úng charset

---

## ? K?T LU?N

**Vi?c chuy?n ??i này s?:**
- ? Tránh l?i encoding
- ? T??ng thích t?t v?i SQL Server
- ? D? debug, d? search
- ? An toàn v?i m?i h? th?ng

**Nh??c ?i?m:**
- ? M?t tính th?m m? ti?ng Vi?t
- ? Khó ??c h?n m?t chút

**Quy?t ??nh cu?i cùng tùy thu?c vào:**
- Yêu c?u c?a khách hàng
- Môi tr??ng production
- ?? ?n ??nh c?n thi?t

---

**Generated:** 2026-01-28  
**Status:** In Progress  
**Priority:** HIGH
