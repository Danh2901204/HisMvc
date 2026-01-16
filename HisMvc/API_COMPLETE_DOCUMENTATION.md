# HIS MVC - API Documentation Complete

## ?? M?c L?c
1. [Public Appointment API](#public-appointment-api)
2. [Admin APIs](#admin-apis)
3. [Reception APIs](#reception-apis)
4. [Doctor APIs](#doctor-apis)
5. [Lab APIs](#lab-apis)
6. [Cashier APIs](#cashier-apis)
7. [Authentication](#authentication)
8. [Data Models](#data-models)

---

## Base Configuration

### Base URLs
```
Production: https://your-domain.com
Development: http://localhost:7239
```

### Authentication
H?u h?t các API n?i b? yêu c?u authentication thông qua ASP.NET Core Identity.
- Cookie-based authentication cho web interface
- JWT/Bearer token có th? ???c implement cho mobile apps

### CORS Configuration
```json
{
  "Portal": {
    "AllowedOrigin": "http://localhost:3000"
  }
}
```

---

## ?? Public Appointment API

### Base Path: `/api/AppointmentsApi`

#### 1. Get Departments
L?y danh sách t?t c? khoa/phòng ban

```http
GET /api/AppointmentsApi/Departments
```

**Response:**
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
    }
  ]
}
```

#### 2. Get Doctors
L?y danh sách bác s? (có th? l?c theo khoa)

```http
GET /api/AppointmentsApi/Doctors?departmentId={departmentId}
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| departmentId | int | No | L?c bác s? theo khoa |

**Response:**
```json
{
  "success": true,
  "doctors": [
    {
      "staffId": 1,
      "fullName": "Nguy?n V?n Minh",
      "departmentId": 1,
      "departmentName": "Khoa Khám b?nh"
    }
  ]
}
```

#### 3. Get Available Time Slots
L?y danh sách khung gi? còn tr?ng

```http
GET /api/AppointmentsApi/AvailableSlots?date={date}&departmentId={departmentId}
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| date | DateOnly | Yes | Ngày khám (yyyy-MM-dd) |
| departmentId | int | No | L?c theo khoa |

**Response:**
```json
{
  "success": true,
  "date": "2026-01-09",
  "slots": [
    {
      "timeSlotId": 1,
      "code": "S1",
      "start": "08:00",
      "end": "09:00",
      "booked": 3,
      "maxCapacity": 10,
      "available": 7
    }
  ]
}
```

#### 4. Book Appointment
??t l?ch h?n m?i

```http
POST /api/AppointmentsApi/Book
Content-Type: application/json
```

**Request Body:**
```json
{
  "fullName": "Nguy?n V?n A",
  "phone": "0123456789",
  "dob": "1990-01-15",
  "gender": 1,
  "departmentId": 1,
  "doctorId": 1,
  "date": "2026-01-09",
  "timeSlotId": 1,
  "note": "Khám s?c kh?e ??nh k?"
}
```

**Request Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| fullName | string | Yes | H? tên b?nh nhân |
| phone | string | Yes | S? ?i?n tho?i (unique) |
| dob | DateOnly | No | Ngày sinh |
| gender | int | No | 0=Unknown, 1=Male, 2=Female, 3=Other |
| departmentId | int | Yes | ID khoa khám |
| doctorId | int | No | ID bác s? |
| date | DateOnly | Yes | Ngày khám |
| timeSlotId | int | Yes | ID khung gi? |
| note | string | No | Ghi chú |

**Response:**
```json
{
  "success": true,
  "message": "Dat lich thanh cong",
  "appointmentCode": "APT20260109143022",
  "appointmentId": 123,
  "date": "09/01/2026"
}
```

#### 5. Check Appointment
Ki?m tra thông tin l?ch h?n

```http
GET /api/AppointmentsApi/Check?code={appointmentCode}
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| code | string | Yes | Mã l?ch h?n |

**Response:**
```json
{
  "success": true,
  "appointment": {
    "code": "APT20260109143022",
    "status": "Booked",
    "date": "09/01/2026",
    "timeSlot": "08:00 - 09:00",
    "patient": {
      "fullName": "Nguy?n V?n A",
      "phone": "0123456789",
      "gender": "Male"
    },
    "department": "Khoa Khám b?nh",
    "doctor": "Nguy?n V?n Minh",
    "note": "Khám s?c kh?e ??nh k?"
  }
}
```

---

## ?? Authentication

### Login
```http
POST /Account/Login
Content-Type: application/x-www-form-urlencoded
```

**Request Body:**
```
email=admin@his.local
password=123456
returnUrl=/
```

**Default Accounts:**
| Email | Password | Role |
|-------|----------|------|
| admin@his.local | 123456 | ADMIN |
| reception@his.local | 123456 | RECEPTION |
| doctor@his.local | 123456 | DOCTOR |
| lab@his.local | 123456 | LAB_TECH |
| cashier@his.local | 123456 | CASHIER |

### Logout
```http
POST /Account/Logout
```

---

## ????? Admin APIs

### Base Path: `/Admin`
**Required Role:** ADMIN

#### Department Management

##### List Departments
```http
GET /Admin/Department/Index
```

##### Create Department
```http
POST /Admin/Department/Create
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
Code=DEPT001
Name=Khoa Test
```

##### Edit Department
```http
POST /Admin/Department/Edit
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
DepartmentId=1
Code=DEPT001
Name=Khoa Updated
```

##### Delete Department
```http
POST /Admin/Department/Delete
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
id=1
```

#### Staff Management

##### List Staff
```http
GET /Admin/Staff/Index
```

##### Create Staff
```http
POST /Admin/Staff/Create
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
FullName=Nguyen Van A
DepartmentId=1
StaffType=DOCTOR
```

##### Edit Staff
```http
POST /Admin/Staff/Edit
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
StaffId=1
FullName=Nguyen Van B
DepartmentId=1
StaffType=DOCTOR
```

##### Delete Staff
```http
POST /Admin/Staff/Delete
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
id=1
```

#### Service Management

##### List Services
```http
GET /Admin/Service/Index
```

##### Create Service
```http
POST /Admin/Service/Create
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
Name=Xét nghi?m máu
Type=LAB
Price=150000
```

**Service Types:**
- LAB: Xét nghi?m
- IMAGING: Ch?n ?oán hình ?nh
- EXAM: Khám

##### Edit Service
```http
POST /Admin/Service/Edit
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
ServiceId=1
Name=Xét nghi?m máu t?ng quát
Type=LAB
Price=200000
```

##### Delete Service
```http
POST /Admin/Service/Delete
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
id=1
```

#### TimeSlot Management

##### List TimeSlots
```http
GET /Admin/TimeSlot/Index
```

##### Create TimeSlot
```http
POST /Admin/TimeSlot/Create
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
Code=S1
Start=08:00
End=09:00
```

##### Edit TimeSlot
```http
POST /Admin/TimeSlot/Edit
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
TimeSlotId=1
Code=S1
Start=08:00
End=09:30
```

##### Delete TimeSlot
```http
POST /Admin/TimeSlot/Delete
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
id=1
```

#### User Management

##### List Users
```http
GET /Admin/User/Index
```

##### Create User
```http
POST /Admin/User/Create
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
userName=newuser@his.local
email=newuser@his.local
password=Password123
role=DOCTOR
staffId=1
```

##### Edit User
```http
POST /Admin/User/Edit
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
id=user-guid-here
email=updated@his.local
role=DOCTOR
staffId=1
```

##### Reset Password
```http
POST /Admin/User/ResetPassword
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
id=user-guid-here
newPassword=NewPassword123
```

##### Delete User
```http
POST /Admin/User/Delete
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
id=user-guid-here
```

---

## ?? Reception APIs

### Base Path: `/Reception`
**Required Role:** RECEPTION, ADMIN

#### Appointment Management

##### View Appointments by Date
```http
GET /Reception/Home/Index?date={date}&status={status}
```
**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| date | DateOnly | No | Default: today (yyyy-MM-dd) |
| status | string | No | Booked, CheckedIn, Cancelled |

##### Create Appointment (Form View)
```http
GET /Reception/Home/Create
```

##### Create Appointment (Submit)
```http
POST /Reception/Home/Create
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
fullName=Nguyen Van A
phone=0123456789
dob=1990-01-15
gender=1
departmentId=1
doctorId=1
slotId=1
date=2026-01-09
note=Khám ??nh k?
```

##### Check-In Appointment
```http
POST /Reception/Home/CheckIn
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
appointmentId=123
```

**Effect:** T?o Encounter m?i v?i status CheckedIn

##### Cancel Appointment
```http
POST /Reception/Home/Cancel
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
appointmentId=123
```

##### Search Patient (AJAX)
```http
GET /Reception/Home/SearchPatient?phone={phone}
```
**Response:**
```json
{
  "found": true,
  "patientId": 1,
  "fullName": "Nguyen Van A",
  "phone": "0123456789",
  "dob": "1990-01-15",
  "gender": "Male"
}
```

#### Patient Management

##### List Patients
```http
GET /Reception/Home/Patients?search={keyword}
```

##### Create Patient (Form)
```http
GET /Reception/Home/CreatePatient
```

##### Create Patient (Submit)
```http
POST /Reception/Home/CreatePatient
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
FullName=Nguyen Van A
Phone=0123456789
Dob=1990-01-15
Gender=1
```

##### Edit Patient (Form)
```http
GET /Reception/Home/EditPatient?id={patientId}
```

##### Edit Patient (Submit)
```http
POST /Reception/Home/EditPatient
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
PatientId=1
FullName=Nguyen Van A Updated
Phone=0123456789
Dob=1990-01-15
Gender=1
```

---

## ????? Doctor APIs

### Base Path: `/Doctor`
**Required Role:** DOCTOR, ADMIN

#### Encounter Management

##### List Encounters
```http
GET /Doctor/Home/Index?status={status}
```
**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| status | string | No | CheckedIn, InService, Completed |

##### Examine Patient (View)
```http
GET /Doctor/Home/Examine?id={encounterId}
```

##### Save Diagnosis & Conclusion
```http
POST /Doctor/Home/Save
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
id=1
diagnosis=Viêm h?ng c?p
conclusion=Kê ??n thu?c, tái khám sau 3 ngày
```

#### Order Management

##### Add Order (Ch? ??nh d?ch v?)
```http
POST /Doctor/Home/AddOrder
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
encounterId=1
serviceId=5
```

##### Cancel Order
```http
POST /Doctor/Home/CancelOrder
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
orderId=10
encounterId=1
```

**Note:** Ch? có th? h?y Order v?i status = Requested

##### Close Encounter (Ch?t l??t khám)
```http
POST /Doctor/Home/Close
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
id=1
```

**Validations:**
- Ph?i có ch?n ?oán (diagnosis)
- Không có Order nào còn status = Requested
- T? ??ng t?o Invoice khi close

##### Reopen Encounter
```http
POST /Doctor/Home/Reopen
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
id=1
```

---

## ?? Lab APIs

### Base Path: `/Lab`
**Required Role:** LAB_TECH, ADMIN

#### Order Management

##### List Pending Orders
```http
GET /Lab/Home/Index?serviceType={type}&date={date}&departmentId={deptId}
```
**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| serviceType | string | No | LAB, IMAGING |
| date | DateOnly | No | Filter by date |
| departmentId | int | No | Filter by department |

##### View Result History
```http
GET /Lab/Home/History?serviceType={type}&date={date}
```
**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| serviceType | string | No | LAB, IMAGING |
| date | DateOnly | No | Default: today |

##### Enter Result (Form)
```http
GET /Lab/Home/Result?id={orderId}
```

##### Save Result
```http
POST /Lab/Home/SaveResult
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
orderId=10
resultText=WBC: 8.5, RBC: 4.8, HGB: 14.5
```

**Effect:** 
- T?o/c?p nh?t OrderResult
- Update Order status = Resulted

##### Clear Result
```http
POST /Lab/Home/ClearResult
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
orderId=10
```

**Effect:**
- Xóa OrderResult
- Update Order status = Requested

---

## ?? Cashier APIs

### Base Path: `/Cashier`
**Required Role:** CASHIER, ADMIN

#### Invoice Management

##### List Invoices
```http
GET /Cashier/Home/Index?status={status}&date={date}
```
**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| status | string | No | Unpaid, Paid |
| date | DateOnly | No | Default: today |

##### List Pending Encounters (Ch?a l?p hóa ??n)
```http
GET /Cashier/Home/Pending?date={date}
```
**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| date | DateOnly | No | Default: today |

**Returns:** Danh sách Encounter v?i status = Completed nh?ng ch?a có Invoice

##### Create Invoice (Form)
```http
GET /Cashier/Home/Create?encounterId={id}
```

##### Create Invoice (Submit)
```http
POST /Cashier/Home/CreateInvoice
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
encounterId=1
note=Thanh toán ??y ??
```

**Calculation:**
- ExamFee: 100,000 VND (hardcoded)
- TotalOrderPrice: Sum of all Service prices in Orders
- TotalAmount: ExamFee + TotalOrderPrice

##### View Invoice Detail
```http
GET /Cashier/Home/Detail?id={invoiceId}
```

##### Process Payment
```http
POST /Cashier/Home/Pay
Content-Type: application/x-www-form-urlencoded
```
**Body:**
```
id=123
```

**Effect:**
- Update Invoice status = Paid
- Set PaidAt = DateTime.UtcNow
- Set PaidBy = Current user

---

## ?? Data Models

### Enums

#### Gender
```csharp
public enum Gender 
{ 
    Unknown = 0, 
    Male = 1, 
    Female = 2, 
    Other = 3 
}
```

#### AppointmentStatus
```csharp
public enum AppointmentStatus 
{ 
    Booked = 1, 
    Cancelled = 9 
}
```

#### EncounterStatus
```csharp
public enum EncounterStatus 
{ 
    CheckedIn = 1,    // ?ã check-in
    InService = 2,    // ?ang khám
    Completed = 8,    // Hoàn thành
    Cancelled = 9     // H?y
}
```

#### OrderStatus
```csharp
public enum OrderStatus 
{ 
    Requested = 1,    // Ch? x? lý
    Resulted = 6,     // ?ã có k?t qu?
    Verified = 7,     // ?ã xác nh?n
    Cancelled = 9     // H?y
}
```

#### InvoiceStatus
```csharp
public enum InvoiceStatus 
{ 
    Unpaid = 1,       // Ch?a thanh toán
    Paid = 2          // ?ã thanh toán
}
```

### Core Entities

#### Department
```json
{
  "departmentId": 1,
  "code": "KB",
  "name": "Khoa Khám b?nh"
}
```

#### Staff
```json
{
  "staffId": 1,
  "fullName": "Nguy?n V?n Minh",
  "departmentId": 1,
  "staffType": "DOCTOR"
}
```

#### Patient
```json
{
  "patientId": 1,
  "fullName": "Nguy?n V?n A",
  "dob": "1990-01-15",
  "gender": 1,
  "phone": "0123456789"
}
```

#### TimeSlot
```json
{
  "timeSlotId": 1,
  "code": "S1",
  "start": "08:00:00",
  "end": "09:00:00"
}
```

#### Appointment
```json
{
  "appointmentId": 1,
  "code": "APT20260109143022",
  "patientId": 1,
  "departmentId": 1,
  "doctorId": 1,
  "date": "2026-01-09",
  "timeSlotId": 1,
  "status": 1,
  "note": "Khám ??nh k?",
  "createdAt": "2026-01-09T14:30:22Z"
}
```

#### Encounter
```json
{
  "encounterId": 1,
  "patientId": 1,
  "appointmentId": 1,
  "doctorId": 1,
  "status": 2,
  "checkInAt": "2026-01-09T08:15:00Z",
  "diagnosis": "Viêm h?ng c?p",
  "conclusion": "Kê ??n thu?c",
  "endAt": "2026-01-09T08:45:00Z"
}
```

#### Service
```json
{
  "serviceId": 1,
  "name": "Công th?c máu (CBC)",
  "type": "LAB",
  "price": 80000
}
```

#### Order
```json
{
  "orderId": 1,
  "encounterId": 1,
  "serviceId": 1,
  "status": 1,
  "orderedAt": "2026-01-09T08:30:00Z",
  "orderedBy": "doctor@his.local"
}
```

#### OrderResult
```json
{
  "orderResultId": 1,
  "orderId": 1,
  "resultText": "WBC: 8.5, RBC: 4.8, HGB: 14.5",
  "resultedAt": "2026-01-09T10:15:00Z",
  "resultedBy": "lab@his.local"
}
```

#### Invoice
```json
{
  "invoiceId": 1,
  "encounterId": 1,
  "invoiceCode": "INV20260109103000",
  "totalAmount": 280000,
  "status": 1,
  "createdAt": "2026-01-09T10:30:00Z",
  "paidAt": null,
  "paidBy": null,
  "note": "Thanh toán ??y ??"
}
```

---

## ?? Workflow Complete

### 1. Patient Booking (Public)
```
POST /api/AppointmentsApi/Book
? Creates: Patient (if new) + Appointment
```

### 2. Reception Check-In
```
POST /Reception/Home/CheckIn
? Creates: Encounter (status = CheckedIn)
```

### 3. Doctor Examination
```
POST /Doctor/Home/Save (diagnosis & conclusion)
POST /Doctor/Home/AddOrder (ch? ??nh xét nghi?m)
? Creates: Orders (status = Requested)
```

### 4. Lab Processing
```
POST /Lab/Home/SaveResult
? Creates: OrderResult
? Updates: Order (status = Resulted)
```

### 5. Doctor Close Encounter
```
POST /Doctor/Home/Close
? Updates: Encounter (status = Completed)
? Auto Creates: Invoice (status = Unpaid)
```

### 6. Cashier Payment
```
POST /Cashier/Home/Pay
? Updates: Invoice (status = Paid)
```

---

## ??? Security & Authorization

### Role-Based Access Control

| Role | Access Areas |
|------|--------------|
| ADMIN | Full access to all areas |
| RECEPTION | Reception area, Patient management |
| DOCTOR | Doctor area, Encounter & Order management |
| LAB_TECH | Lab area, Order result management |
| CASHIER | Cashier area, Invoice management |
| PHARMACIST | (Future use) |
| MANAGER | (Future use) |

### Anti-Forgery Tokens
T?t c? POST requests yêu c?u anti-forgery token:
```html
<form method="post">
    @Html.AntiForgeryToken()
    <!-- form fields -->
</form>
```

---

## ?? Error Handling

### HTTP Status Codes
- `200 OK` - Request successful
- `400 Bad Request` - Invalid input
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not authorized
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

### Error Response Format
```json
{
  "success": false,
  "message": "Error description in Vietnamese"
}
```

### TempData Messages (Web Interface)
```csharp
TempData["Success"] = "Thao tac thanh cong!";
TempData["Error"] = "Co loi xay ra!";
```

---

## ?? Testing

### Test Accounts
```
Admin:      admin@his.local / 123456
Reception:  reception@his.local / 123456
Doctor:     doctor@his.local / 123456
Lab Tech:   lab@his.local / 123456
Cashier:    cashier@his.local / 123456
```

### Sample Test Data
```sql
-- Departments: KB, NOI, TMH
-- Staff: 3 doctors
-- TimeSlots: S1 (08:00-09:00), S2 (09:00-10:00), S3 (10:00-11:00), C1 (13:00-14:00)
-- Services: CBC (80,000), Blood Glucose (50,000), Chest X-Ray (120,000)
```

---

## ?? Additional Resources

### Database Schema
- S? d?ng Entity Framework Core Code First
- Connection String: `Server=.;Database=HIS_MVC_DB;Trusted_Connection=True;TrustServerCertificate=True`
- Migrations: Available in `/Migrations` folder

### Frontend
- Bootstrap 5
- Bootstrap Icons
- jQuery (for AJAX calls)
- Responsive design

### Configuration Files
- `appsettings.json` - Application settings
- `Program.cs` - Application startup
- `AppDbContext.cs` - Database context

---

## ?? Future API Enhancements

### Recommendations for Production:

1. **Add JWT Authentication for Mobile Apps**
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });
```

2. **Implement Pagination**
```csharp
public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
```

3. **Add Search & Filter APIs**
```csharp
GET /api/Patients/Search?keyword={keyword}&page={page}
```

4. **Add Real-time Notifications (SignalR)**
```csharp
hubConnection.on("NewAppointment", (appointment) => { ... });
```

5. **Add Export APIs**
```csharp
GET /Admin/Reports/ExportInvoices?from={date}&to={date}&format=excel
```

6. **Add Audit Trail API**
```csharp
GET /Admin/AuditLog?userId={id}&from={date}&to={date}
```

7. **Add Dashboard Statistics API**
```csharp
GET /api/Statistics/Dashboard
```

8. **Add Prescription Management**
```csharp
POST /Doctor/Prescription/Create
GET /Doctor/Prescription/{encounterId}
```

9. **Add Medical Records API**
```csharp
GET /api/MedicalRecords/{patientId}
```

10. **Add Appointment Reminders (Background Job)**
```csharp
// Send SMS/Email reminders before appointment time
```

---

## ?? Support

For technical support or questions:
- Email: support@his-mvc.local
- Documentation: [README.md](README.md)
- Integration Guide: [API_INTEGRATION_GUIDE.md](API_INTEGRATION_GUIDE.md)

---

**Version:** 1.0.0  
**Last Updated:** January 2026  
**Framework:** .NET 10  
**Database:** SQL Server
