# ?? FIX: Web không l?y ???c d? li?u t? HIS API

## ? V?n ??

Web Node.js c?a b?n ?ang s? d?ng **MOCK DATA** thay vì g?i API th?t t? HIS MVC.

## ? Gi?i Pháp

### B??c 1: S?a file `.env`

M? file: `C:\UTT\Thuctap\WebBenhvien\web-bvien\.env`

S?a dòng này:
```env
USE_MOCK=true
```

Thành:
```env
USE_MOCK=false
```

File `.env` ??y ?? nên nh? sau:
```env
PORT=3000
HOST=localhost
NODE_ENV=development
USE_MOCK=false
HIS_API_URL=http://localhost:7239/api/AppointmentsApi
HIS_API_KEY=
HOSPITAL_NAME=B?nh Vi?n
```

### B??c 2: ??m b?o HIS MVC ?ang ch?y

1. **Ch?y HIS MVC:**
   - M? Visual Studio
   - Run project HisMvc (F5)
   - Ki?m tra xem có ch?y ? `http://localhost:7239` không

2. **Test API tr?c ti?p:**
   
   M? browser và truy c?p:
   ```
   http://localhost:7239/api/AppointmentsApi/Departments
   ```
   
   N?u thành công, b?n s? th?y JSON data nh?:
   ```json
   {
     "success": true,
     "departments": [
       {
         "departmentId": 1,
         "code": "KB",
         "name": "Khoa Khám b?nh"
       }
     ]
   }
   ```

### B??c 3: Restart Web Node.js

```bash
# Stop server (Ctrl+C)
# Restart
npm run dev
```

ho?c

```bash
# Restart nodemon
rs
```

### B??c 4: Test l?i Web

1. M? browser: `http://localhost:3000`
2. Vào trang ??t l?ch
3. Ki?m tra xem dropdown **Khoa** và **Bác s?** có hi?n th? d? li?u không

---

## ?? Ki?m Tra L?i

### L?i CORS

N?u th?y l?i CORS trong Console:
```
Access to XMLHttpRequest at 'http://localhost:7239/api/...' from origin 'http://localhost:3000' has been blocked by CORS policy
```

**Gi?i pháp:** CORS ?ã ???c config trong HIS MVC (`Program.cs`), nh?ng ki?m tra l?i:

```csharp
// Program.cs
var portalOrigin = builder.Configuration["Portal:AllowedOrigin"] ?? "http://localhost:3000";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Portal", p =>
        p.WithOrigins(portalOrigin)
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// ...

app.UseCors("Portal");
```

### L?i Connection Refused

N?u th?y l?i:
```
Error: connect ECONNREFUSED 127.0.0.1:7239
```

**Nguyên nhân:** HIS MVC ch?a ch?y ho?c ch?y sai port.

**Gi?i pháp:** 
1. Run HIS MVC trong Visual Studio
2. Ki?m tra port trong Properties > launchSettings.json
3. Update `HIS_API_URL` trong `.env` n?u c?n

---

## ?? Test API b?ng Postman/cURL

### Test Departments

```bash
curl http://localhost:7239/api/AppointmentsApi/Departments
```

### Test Doctors

```bash
curl http://localhost:7239/api/AppointmentsApi/Doctors
```

### Test Doctors by Department

```bash
curl "http://localhost:7239/api/AppointmentsApi/Doctors?departmentId=1"
```

### Test TimeSlots

```bash
curl "http://localhost:7239/api/AppointmentsApi/AvailableSlots?date=2026-01-15"
```

---

## ?? Debug Web Routes

N?u v?n không work, ki?m tra logs:

### Server Logs (Node.js)

```
[2026-01-09T09:00:00.000Z] GET /api/departments
[Department GET] Error: connect ECONNREFUSED 127.0.0.1:7239
```

? HIS MVC ch?a ch?y

```
[2026-01-09T09:00:00.000Z] GET /api/departments
[Department GET] Success: Retrieved 3 departments
```

? Thành công!

### HIS Logs

Ki?m tra trong Visual Studio Output window khi call API:
```
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:7239/api/AppointmentsApi/Departments
```

---

## ? Checklist

- [ ] HIS MVC ?ang ch?y ? `http://localhost:7239`
- [ ] `.env` có `USE_MOCK=false`
- [ ] `.env` có `HIS_API_URL=http://localhost:7239/api/AppointmentsApi`
- [ ] CORS ???c config ?úng trong `appsettings.json`
- [ ] Web Node.js ?ã restart
- [ ] Test API b?ng browser/Postman thành công
- [ ] Không có l?i trong Console (F12)

---

## ?? N?u V?n Không Work

### Option 1: T?m dùng Mock Data

Gi? `USE_MOCK=true` ?? test UI tr??c, sau ?ó fix API sau.

### Option 2: Check Network Tab

1. M? Developer Tools (F12)
2. Tab **Network**
3. Load l?i trang
4. Tìm request ??n `/api/departments` ho?c `/api/doctors`
5. Xem Response và Status Code
6. G?i screenshot ?? debug

### Option 3: Test tr?c ti?p trong code

Thêm log vào `department.routes.js`:

```javascript
router.get('/', async (req, res, next) => {
  try {
    console.log('?? Calling HIS API:', `${HIS_API}/Departments`);
    console.log('?? USE_MOCK:', process.env.USE_MOCK);
    
    const response = await axios.get(`${HIS_API}/Departments`, { timeout: 10000 });
    console.log('? Response:', response.data);
    
    // ... rest of code
  } catch (error) {
    console.error('? Error details:', {
      message: error.message,
      code: error.code,
      response: error.response?.data
    });
    // ... rest of code
  }
});
```

---

## ?? C?n H? Tr??

N?u v?n g?p v?n ??, cung c?p thông tin sau:

1. Screenshot l?i trong Console (F12)
2. Screenshot l?i trong Terminal (Node.js)
3. Screenshot Visual Studio Output khi call API
4. N?i dung file `.env`
5. Port HIS MVC ?ang ch?y
