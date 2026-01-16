# ? FIX HOÀN T?T - H??NG D?N TEST API

## ?? Nh?ng Gì ?ã S?a

### 1. **Thêm API Controller Mapping**
Trong `Program.cs`, ?ã thêm:
```csharp
app.MapControllers(); // Map API controllers
```

### 2. **S?a Middleware Order**
```csharp
app.UseRouting();
app.UseCors("Portal");      // CORS ph?i sau UseRouting
app.UseAuthentication();
app.UseAuthorization();
```

### 3. **Thêm Exception Handling**
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
```

---

## ?? B??C TI?P THEO

### B??c 1: **RESTART HIS MVC**

?? **QUAN TR?NG:** Ph?i restart ?ng d?ng ?? áp d?ng thay ??i!

**Option A: Trong Visual Studio**
1. Stop debugging (Shift+F5)
2. Start l?i (F5)

**Option B: Hot Reload (N?u ?ang debug)**
1. L?u file (Ctrl+S)
2. Hot Reload s? t? ??ng apply

### B??c 2: **KI?M TRA PORT**

Khi HIS MVC ch?y, ki?m tra Output window:
```
Now listening on: https://localhost:7239
Now listening on: http://localhost:5000
```

N?u port khác `7239`, c?p nh?t trong web `.env`:
```env
HIS_API_URL=http://localhost:YOUR_PORT/api/AppointmentsApi
```

### B??c 3: **TEST API QUA BROWSER**

M? browser và test t?ng endpoint:

#### ? Test 1: Departments
```
http://localhost:7239/api/AppointmentsApi/Departments
```

**Response mong ??i:**
```json
{
  "success": true,
  "departments": [
    {
      "departmentId": 1,
      "code": "KB",
      "name": "Khoa Khám b?nh"
    },
    {
      "departmentId": 2,
      "code": "NOI",
      "name": "N?i t?ng h?p"
    },
    {
      "departmentId": 3,
      "code": "TMH",
      "name": "Tai M?i H?ng"
    }
  ]
}
```

#### ? Test 2: Doctors
```
http://localhost:7239/api/AppointmentsApi/Doctors
```

**Response mong ??i:**
```json
{
  "success": true,
  "doctors": [
    {
      "staffId": 1,
      "fullName": "Nguy?n V?n Minh",
      "departmentId": 1,
      "departmentName": "Khoa Khám b?nh"
    },
    {
      "staffId": 2,
      "fullName": "Tr?n Th? H??ng",
      "departmentId": 1,
      "departmentName": "Khoa Khám b?nh"
    }
  ]
}
```

#### ? Test 3: Doctors by Department
```
http://localhost:7239/api/AppointmentsApi/Doctors?departmentId=1
```

#### ? Test 4: Available Slots
```
http://localhost:7239/api/AppointmentsApi/AvailableSlots?date=2026-01-15
```

**Response mong ??i:**
```json
{
  "success": true,
  "date": "2026-01-15",
  "slots": [
    {
      "timeSlotId": 1,
      "code": "S1",
      "start": "08:00",
      "end": "09:00",
      "booked": 0,
      "maxCapacity": 10,
      "available": 10
    }
  ]
}
```

### B??c 4: **KI?M TRA WEB NODE.JS**

1. **??m b?o `.env` ?ã s?a:**
```env
USE_MOCK=false
HIS_API_URL=http://localhost:7239/api/AppointmentsApi
```

2. **Restart Web:**
```bash
# Stop: Ctrl+C
# Start l?i:
npm run dev
```

3. **Test qua web:**
   - M? `http://localhost:3000`
   - Vào trang ??t l?ch
   - M? Developer Tools (F12) ? Console
   - Xem dropdown **Khoa** và **Bác s?**

### B??c 5: **CHECK CONSOLE LOGS**

**Trong Web (Browser Console):**
```javascript
// N?u thành công, s? th?y:
Departments loaded: [...]
Doctors loaded: [...]

// N?u l?i CORS, s? th?y:
Access to XMLHttpRequest at '...' has been blocked by CORS policy
```

**Trong HIS MVC (Visual Studio Output):**
```
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:7239/api/AppointmentsApi/Departments
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'HisMvc.Controllers.Api.AppointmentsApiController.GetDepartments'
```

**Trong Web Node.js (Terminal):**
```
[2026-01-09T14:00:00.000Z] GET /api/departments
?? Calling HIS API: http://localhost:7239/api/AppointmentsApi/Departments
? Response: { success: true, departments: [...] }
```

---

## ?? TROUBLESHOOTING

### ? L?i 1: "404 Not Found"

**Nguyên nhân:** API controller không ???c map.

**Gi?i pháp:** ?ã fix b?ng `app.MapControllers()` ?

---

### ? L?i 2: CORS Error

**L?i ??y ??:**
```
Access to XMLHttpRequest at 'http://localhost:7239/api/AppointmentsApi/Departments' 
from origin 'http://localhost:3000' has been blocked by CORS policy: 
No 'Access-Control-Allow-Origin' header is present on the requested resource.
```

**Gi?i pháp:**
1. ??m b?o `app.UseCors("Portal")` ??t sau `app.UseRouting()` ?
2. Ki?m tra `appsettings.json`:
```json
{
  "Portal": {
    "AllowedOrigin": "http://localhost:3000"
  }
}
```
3. Restart HIS MVC

---

### ? L?i 3: "Connection Refused"

**L?i trong Node.js:**
```
Error: connect ECONNREFUSED 127.0.0.1:7239
```

**Nguyên nhân:** HIS MVC ch?a ch?y ho?c ch?y sai port.

**Gi?i pháp:**
1. Ch?y HIS MVC trong Visual Studio
2. Ki?m tra port trong Output window
3. C?p nh?t `.env` n?u port khác

---

### ? L?i 4: "Empty Array"

**Response:**
```json
{
  "success": true,
  "departments": []
}
```

**Nguyên nhân:** Database ch?a có seed data.

**Gi?i pháp:**
```bash
# Delete database
dotnet ef database drop -f

# Recreate
dotnet ef database update

# Run app ?? seed data
```

---

## ?? KI?M TRA DATABASE

### SQL Query ?? verify data:

```sql
-- Check Departments
SELECT * FROM Departments;
-- Expected: 3 rows (KB, NOI, TMH)

-- Check Staffs
SELECT * FROM Staffs WHERE StaffType = 'DOCTOR';
-- Expected: 3 doctors

-- Check TimeSlots
SELECT * FROM TimeSlots;
-- Expected: 4 slots (S1, S2, S3, C1)

-- Check Services
SELECT * FROM Services;
-- Expected: 3 services
```

---

## ? CHECKLIST CU?I CÙNG

- [ ] HIS MVC ?ã restart sau khi s?a `Program.cs`
- [ ] Test API qua browser thành công (c? 4 endpoints)
- [ ] File `.env` có `USE_MOCK=false`
- [ ] Web Node.js ?ã restart
- [ ] Không có l?i CORS trong Console
- [ ] Th?y d? li?u khoa trong dropdown
- [ ] Th?y d? li?u bác s? trong dropdown
- [ ] Console log không có l?i màu ??

---

## ?? N?U T?T C? ??U WORK

B?n s? th?y:

**1. Browser (HIS API):**
```json
{
  "success": true,
  "departments": [...]
}
```

**2. Web (Dropdown Khoa):**
```
[Ch?n khoa]
Khoa Khám b?nh
N?i t?ng h?p
Tai M?i H?ng
```

**3. Web (Dropdown Bác s?):**
```
[Ch?n bác s?]
Nguy?n V?n Minh
Tr?n Th? H??ng
Ph?m ??c Long
```

**4. Console (Không có l?i):**
```javascript
? Departments loaded successfully
? Doctors loaded successfully
```

---

## ?? C?N H? TR??

N?u v?n g?p l?i, cung c?p:

1. **Screenshot Browser:** Test API `/Departments`
2. **Screenshot Console (F12):** Trong web ??t l?ch
3. **Screenshot Terminal:** Node.js logs
4. **Screenshot Output:** Visual Studio khi call API
5. **N?i dung file `.env`**

---

## ?? NEXT STEPS

Sau khi API ho?t ??ng:

1. ? Test ??t l?ch trên web
2. ? Test tra c?u l?ch h?n
3. ? Test check-in (Reception)
4. ? Test khám b?nh (Doctor)
5. ? Test xét nghi?m (Lab)
6. ? Test thanh toán (Cashier)

---

**Good luck! ??**
