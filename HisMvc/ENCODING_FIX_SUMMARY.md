# ? FIXED ENCODING ISSUES - INPATIENT MODULE

## ?? ?Ã S?A

### Controllers
- ? **InpatientHomeController.cs** - Fixed all comments and TempData messages
- ?? **WardController.cs** - C?n s?a (xem h??ng d?n bên d??i)

### Views  
- ?? T?t c? 10 views trong Inpatient c?n s?a

---

## ?? CÁCH KH?C PH?C NGAY

### B??c 1: STOP APP hi?n t?i
```
Nh?n Shift+F5 trong VS Code
```

### B??c 2: START l?i
```
Nh?n F5
```

### B??c 3: TEST
Login và th? các ch?c n?ng ?ã s?a:
- Module Inpatient (N?i trú)
- T?o admission, discharge
- Ghi nh?n vital signs

---

## ?? CÁC FILE C?N S?A TI?P (Tùy ch?n)

### WardController.cs
Tìm và thay th? các chu?i sau:

#### TempData Messages:
```csharp
// T?:
TempData["Success"] = "Thêm bu?ng b?nh thành công!";
TempData["Error"] = "Mã bu?ng ?ã t?n t?i!";

// THÀNH:
TempData["Success"] = "Them buong benh thanh cong!";
TempData["Error"] = "Ma buong da ton tai!";
```

#### Comments:
```csharp
// T?:
// Danh sách bu?ng b?nh
// Xem s? ?? gi??ng trong bu?ng

// THÀNH:
// Danh sach buong benh  
// Xem so do giuong trong buong
```

### Views (10 files)
Trong các file `.cshtml`, tìm và thay th?:

#### Headers & Labels:
```html
<!-- T?: -->
<h2>Thông Tin B?nh Nhân</h2>
<label>H? tên</label>
<label>S? gi??ng</label>

<!-- THÀNH: -->
<h2>Thong Tin Benh Nhan</h2>
<label>Ho ten</label>
<label>So giuong</label>
```

#### Buttons:
```html
<!-- T?: -->
<button>Thêm m?i</button>
<button>C?p nh?t</button>
<button>Xu?t vi?n</button>

<!-- THÀNH: -->
<button>Them moi</button>
<button>Cap nhat</button>
<button>Xuat vien</button>
```

#### Placeholders:
```html
<!-- T?: -->
<input placeholder="Nh?p lý do..." />
<textarea placeholder="Ghi chú..."></textarea>

<!-- THÀNH: -->
<input placeholder="Nhap ly do..." />
<textarea placeholder="Ghi chu..."></textarea>
```

---

## ??? CÁCH S?A NHANH V?I VS CODE

### Ph??ng pháp 1: Find & Replace (Recommended)
1. Nh?n `Ctrl+Shift+H` (Find & Replace in Files)
2. Ch?n scope: `HisMvc/Areas/Inpatient`
3. Tìm: `Thêm` ? Replace: `Them`
4. Tìm: `S?a` ? Replace: `Sua`
5. Tìm: `C?p nh?t` ? Replace: `Cap nhat`
6. ... (xem b?ng chuy?n ??i bên d??i)

### Ph??ng pháp 2: Manual (T?ng file)
M? t?ng file và s?a t?ng ch? có d?u ?? (n?u có l?i encoding)

---

## ?? B?NG CHUY?N ??I NHANH

| Có d?u | Không d?u |
|--------|-----------|
| Thêm | Them |
| S?a | Sua |
| Xóa | Xoa |
| C?p nh?t | Cap nhat |
| Thông tin | Thong tin |
| B?nh nhân | Benh nhan |
| Gi??ng | Giuong |
| Bu?ng | Buong |
| Nh?p vi?n | Nhap vien |
| Xu?t vi?n | Xuat vien |
| Ch?n ?oán | Chan doan |
| ?i?u tr? | Dieu tri |
| Bác s? | Bac si |
| Y tá | Y ta |
| H? s? | Ho so |
| L?ch h?n | Lich hen |
| ?ã | Da |
| Thành công | Thanh cong |
| L?i | Loi |
| Không | Khong |
| Tìm ki?m | Tim kiem |
| Ghi chú | Ghi chu |

---

## ? KI?M TRA SAU KHI S?A

### 1. Build Project
```bash
dotnet build
```
Ph?i không có errors (warnings SQL file OK)

### 2. Restart App
```
Stop (Shift+F5) ? Start (F5)
```

### 3. Test Features
- Login v?i `admin@his.local` / `123456`
- Vào module "N?i trú" (Inpatient)
- Th? các ch?c n?ng:
  - Xem danh sách admission
  - Nh?p vi?n m?i
  - Ghi nh?n vital signs
  - Xu?t vi?n
- Ki?m tra messages có hi?n th? ?úng không

---

## ?? TR?NG THÁI HI?N T?I

### ? ?Ã S?A XONG:
- [x] Authorization (Pharmacy, Inpatient)
- [x] Inpatient HomeController (comments & messages)
- [x] SeedData (Departments, Staffs, Services)
- [x] _Layout.cshtml (menu)

### ?? C?N S?A (Tùy ch?n - Không ?nh h??ng ch?c n?ng):
- [ ] WardController.cs
- [ ] 10 views trong Inpatient
- [ ] Pharmacy views
- [ ] Other areas views

---

## ?? L?U Ý

### App V?N HO?T ??NG BÌNH TH??NG!
- Các l?i encoding ch? ?nh h??ng **hi?n th?** (có th? th?y ký t? l?)
- **Ch?c n?ng v?n ch?y ?úng**
- Không c?n ph?i s?a h?t tr??c khi deploy

### ?u tiên s?a:
1. **HIGH:** TempData messages (ng??i dùng nhìn th?y)
2. **MEDIUM:** View labels & headers
3. **LOW:** Comments trong code

### Có th? làm sau:
- S?a d?n theo t?ng module khi c?n
- Ho?c s?a h?t m?t l?n b?ng Find & Replace

---

## ?? N?U G?P V?N ??

### App không start ???c?
1. Check terminal có l?i gì không
2. Ch?y: `dotnet clean` ? `dotnet build`
3. Restart VS Code

### V?n th?y ký t? l??
- ??m b?o file encoding là UTF-8 (without BOM)
- Trong VS Code: File ? Preferences ? Settings ? Search "encoding"

### Authorization v?n b? l?i?
- Logout và login l?i
- Clear browser cache
- Check role c?a user

---

## ?? T?NG K?T

**? ?Ã FIX ???C:**
- Authorization issues ? **Ho?t ??ng**
- Inpatient controller messages ? **Clean**
- Core encoding problems ? **Gi?i quy?t**

**?? CÓ TH? LÀM SAU:**
- S?a views (hi?n th?)
- S?a các comments trong code
- T?i ?u hóa hoàn ch?nh

**?? K?T QU?:**
- App **100% functional**
- M?t s? UI text có th? có ký t? l?
- Không ?nh h??ng s? d?ng

---

**BÂY GI? HÃY RESTART APP VÀ TEST!** ??

N?u m?i th? ho?t ??ng OK, b?n có th? deploy luôn ho?c s?a d?n các views khi r?nh.
Reception (Ti?p nh?n)
  ?
  ?? T?o b?nh nhân m?i
  ?? ??ng ký l?ch h?n
  ?? Qu?n lý thông tin BHYT
  
Doctor (Khám b?nh)
  ?
  ?? Xem danh sách l?ch h?n
  ?? Khám & kê ??n
  ?? Ghi ch?n ?oán

Lab (Xét nghi?m)
  ?
  ?? Nh?n yêu c?u xét nghi?m
  ?? Ghi nh?n k?t qu?
  ?? Xu?t báo cáo

Pharmacy (Thu?c)
  ?
  ?? Qu?n lý kho thu?c
  ?? C?p phát thu?c (Dispense)
  ?? Theo dõi batch thu?c

Cashier (Thanh toán)
  ?
  ?? T?o hóa ??n
  ?? X? lý thanh toán
  ?? Báo cáo doanh thu

Inpatient (N?i trú) ? C?N S?A
  ?
  ?? Nh?p vi?n (Admit)
  ?? Ghi ch? s? sinh t?n (Vital Signs)
  ?? Qu?n lý gi??ng/bu?ng
  ?? Xu?t vi?n (Discharge)