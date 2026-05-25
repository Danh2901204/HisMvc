# 🏥 HIS Management System

## Hospital Information System - Hệ Thống Quản Lý Bệnh Viện

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-blue)](https://docs.microsoft.com/aspnet/core/mvc/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-green)](https://docs.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927)](https://www.microsoft.com/sql-server)

Hệ thống quản lý bệnh viện toàn diện với các chức năng: Quản lý lịch hẹn, Khám bệnh, Xét nghiệm, Thu ngân và API đặt lịch cho khách hàng.

---

## 📋 Mục Lục

- [Tính Năng](#-tính-năng)
- [Công Nghệ Sử Dụng](#-công-nghệ-sử-dụng)
- [Cấu Trúc Project](#-cấu-trúc-project)
- [Cài Đặt](#-cài-đặt)
- [Cấu Hình](#-cấu-hình)
- [Chạy Ứng Dụng](#-chạy-ứng-dụng)
- [Tài Khoản Mặc Định](#-tài-khoản-mặc-định)
- [API Documentation](#-api-documentation)
- [Database Schema](#-database-schema)
- [Screenshots](#-screenshots)
- [Hướng Dẫn Phát Triển](#-hướng-dẫn-phát-triển)
- [Troubleshooting](#-troubleshooting)
- [License](#-license)

---

## ✨ Tính Năng

### 🔐 **Quản Lý Người Dùng & Phân Quyền**
- Hệ thống 7 roles: Admin, Reception, Doctor, Lab Technician, Pharmacist, Cashier, Manager
- Authentication & Authorization với ASP.NET Core Identity
- Quản lý tài khoản và reset mật khẩu

### 📅 **Lễ Tân (Reception)**
- Quản lý thông tin bệnh nhân
- Đặt lịch hẹn khám bệnh
- Check-in bệnh nhân
- Xem và quản lý lịch hẹn theo ngày
- Tìm kiếm và lọc bệnh nhân

### 👨‍⚕️ **Bác Sĩ (Doctor)**
- Xem danh sách bệnh nhân chờ khám
- Khám bệnh và ghi chẩn đoán
- Chỉ định xét nghiệm/dịch vụ
- Kết luận và hoàn tất lượt khám
- Xem lịch sử khám bệnh

### 🔬 **Xét Nghiệm (Lab)**
- Quản lý chỉ định xét nghiệm (LAB & IMAGING)
- Nhập kết quả xét nghiệm
- Xác nhận kết quả
- Lọc theo khoa, ngày, loại dịch vụ
- Xem lịch sử kết quả

### 💰 **Thu Ngân (Cashier)**
- Xem danh sách lượt khám chờ thanh toán
- Tạo hóa đơn tự động
- Thu tiền và in hóa đơn
- Xem lịch sử thanh toán
- Lọc theo ngày và trạng thái

### 💊 **Dược (Pharmacy)**
- Xem đơn thuốc chờ cấp phát (bước 9 luồng KCB)
- Cấp phát thuốc theo FEFO sau thanh toán cuối
- Quản lý danh mục thuốc, lô, tồn kho
- Lịch sử cấp phát

### 🏥 **Nội trú (Inpatient)**
- Nhập viện, theo dõi bệnh nhân
- Y lệnh, phẫu thuật/thủ thuật, sinh hiệu
- Quản lý buồng/giường (Ward/Bed map)
- Xuất viện và tóm tắt bệnh án

### 🛡️ **BHYT & Giám định**
- Cấu hình tỷ lệ chi trả theo loại thẻ
- Tính BHYT khi thu ngân (ExamFee + CLS + thuốc)
- Tạo hồ sơ giám định, xuất XML (130/QĐ-BHXH)
- Quản lý trạng thái claim (Pending → Submitted → Approved)

### 🔧 **Admin**
- Quản lý Departments (Khoa/Phòng ban)
- Quản lý Staff (Nhân viên/Bác sĩ)
- Quản lý Services (Dịch vụ/Xét nghiệm)
- Quản lý TimeSlots (Khung giờ khám)
- Quản lý bệnh nhân (HSBA), dị ứng, tiền sử bệnh
- Cấu hình BHYT, giám định BHYT
- Quản lý Users & Roles, reset mật khẩu

### 📺 **Hàng đợi & ICD-10**
- Màn hình queue công khai (`/Queue`) theo khoa/phòng
- Bác sĩ gọi BN, cấp STT sau thu phí khám
- Tra cứu ICD-10, tự gợi ý mã từ chẩn đoán (TT 56/2017)

### 🌐 **Public API cho Đặt Lịch**
- API RESTful cho website/app khách hàng
- Đặt lịch khám trực tuyến
- Tra cứu lịch hẹn
- Xem khung giờ còn trống
- CORS enabled

---

## 🛠 Công Nghệ Sử Dụng

### **Backend**
- **.NET 10.0** - Framework chính
- **ASP.NET Core MVC** - Web framework với Area-based architecture
- **Entity Framework Core 10.0** - ORM
- **ASP.NET Core Identity** - Authentication & Authorization
- **SQL Server** - Database

### **Frontend**
- **Razor Pages/Views** - Server-side rendering
- **Bootstrap 5** - UI Framework
- **Bootstrap Icons** - Icon library
- **jQuery** - JavaScript library
- **SweetAlert2** - Notifications (optional)

### **Architecture**
- **MVC Pattern** với Areas
- **Repository Pattern** (via DbContext)
- **Dependency Injection**
- **RESTful API**

---

## 📁 Cấu Trúc Project

```
HisMvc/
│
├── Areas/                          # MVC theo phòng ban (Area-based)
│   ├── Admin/                      # Quản trị: danh mục, BN, BHYT, user
│   │   ├── Controllers/
│   │   ├── Models/AdminViewModels.cs
│   │   ├── Services/AdminViewService.cs
│   │   └── Views/
│   ├── Reception/                  # Lễ tân — check-in, lịch hẹn
│   │   ├── Models/ReceptionViewModels.cs
│   │   ├── Services/ReceptionViewService.cs
│   │   └── ...
│   ├── Cashier/                    # Thu ngân — thu phí khám & thanh toán cuối
│   ├── Doctor/                     # Bác sĩ — khám, chỉ định CLS, kết luận
│   ├── Lab/                        # Xét nghiệm / CĐHA
│   ├── Pharmacy/                   # Dược — cấp thuốc
│   └── Inpatient/                  # Nội trú — nhập viện, y lệnh, phẫu thuật, buồng
│
├── Controllers/
│   ├── AccountController.cs        # Authentication
│   ├── HomeController.cs           # Landing page
│   ├── QueueController.cs          # Màn hình hàng đợi
│   └── Api/AppointmentsApiController.cs
│
├── Data/                           # DbContext, Seed, migrations
├── Entities/                       # EF entities (Core, Orders, Invoice, Inpatient…)
├── Models/                         # DTO dùng chung (AppRoles, WorkflowResult…)
│
├── Services/
│   ├── Workflow/                   # Luồng KCB ngoại trú 10 bước (BYT)
│   ├── OutpatientWorkflowService.cs
│   ├── InsuranceService.cs         # Tính BHYT, giám định, XML
│   ├── Icd10Service.cs             # Tra cứu / parse ICD-10
│   └── CurrentStaffService.cs
│
├── Scripts/                        # Công cụ hỗ trợ (không build vào app)
│   ├── ViConvert/                  # Chuyển UI sang tiếng Việt có dấu
│   ├── Sync-HisMvcDatabases.ps1
│   └── Fix-Patient-Gender.sql
│
├── Views/                          # Layout, Account, Queue
├── wwwroot/
├── Program.cs                      # DI: ViewService + WorkflowService
└── HisMvc.csproj
```

**Mô hình MVC trong từng Area:**

| GET | POST (luồng KCB) |
|-----|------------------|
| Controller → `{Area}ViewService` → ViewModel → View | Controller → `OutpatientWorkflowService` → Redirect |

Chi tiết: [`HisMvc/PROJECT_STRUCTURE.md`](HisMvc/PROJECT_STRUCTURE.md) · [`HisMvc/LUONG_KCB_SIMPLE.md`](HisMvc/LUONG_KCB_SIMPLE.md) · [`HisMvc/CHANGELOG_LUONG_BYT.md`](HisMvc/CHANGELOG_LUONG_BYT.md)

---

## 🚀 Cài Đặt

### **Yêu Cầu Hệ Thống**

- **.NET 10 SDK** hoặc mới hơn
- **SQL Server 2019** hoặc mới hơn (hoặc SQL Server Express/LocalDB)
- **Visual Studio 2022** (17.12+) hoặc **VS Code** với C# extension
- **Git** (optional)

### **Các Bước Cài Đặt**

#### **1. Clone Repository**
```bash
git clone https://github.com/Danh2901204/HisMvc.git
cd HisMvc
```

#### **2. Restore Dependencies**
```bash
cd HisMvc
dotnet restore
```

#### **3. Cấu Hình Connection String**

Mở `appsettings.json` và cập nhật connection string:

```json
{
  "ConnectionStrings": {
    "Default": "Server=YOUR_SERVER;Database=HIS_MVC_DB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Ví dụ:**
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=HIS_MVC_DB;...`
- LocalDB: `Server=(localdb)\\mssqllocaldb;Database=HIS_MVC_DB;...`
- Remote Server: `Server=192.168.1.100;Database=HIS_MVC_DB;User Id=sa;Password=YourPassword;...`

#### **4. Tạo Database**

```bash
# Add migration (nếu chưa có)
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

**Lưu ý:** Seed data sẽ tự động chạy khi khởi động ứng dụng lần đầu.

#### **5. Build Project**
```bash
dotnet build
```

---

## ⚙️ Cấu Hình

### **appsettings.json**

```json
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=HIS_MVC_DB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Portal": {
    "AllowedOrigin": "http://localhost:3000"  // CORS cho API
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### **CORS Configuration**

Để cho phép nhiều domain truy cập API, sửa trong `Program.cs`:

```csharp
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Portal", p =>
        p.WithOrigins(
            "http://localhost:3000",
            "https://your-website.com",
            "https://your-mobile-app.com"
         )
         .AllowAnyHeader()
         .AllowAnyMethod());
});
```

---

## ▶️ Chạy Ứng Dụng

### **1. Chạy bằng Visual Studio**
- Mở `HisMvc.sln`
- Chọn profile `https` hoặc `http`
- Nhấn `F5` hoặc click nút **Run**

### **2. Chạy bằng Command Line**
```bash
dotnet run --project HisMvc
```

### **3. Chạy với Hot Reload**
```bash
dotnet watch run
```

Ứng dụng sẽ chạy tại:
- **HTTPS**: `https://localhost:7239`
- **HTTP**: `http://localhost:5000`

---

## 👥 Tài Khoản Mặc Định

Sau khi seed data, sử dụng các tài khoản sau để đăng nhập:

| Role | Email | Password | Mô tả |
|------|-------|----------|-------|
| **Admin** | admin@his.local | 123456 | Quản trị viên |
| **Reception** | reception@his.local | 123456 | Lễ tân |
| **Doctor** | doctor@his.local | 123456 | Bác sĩ |
| **Lab Tech** | lab@his.local | 123456 | Xét nghiệm |
| **Pharmacist** | pharmacy@his.local | 123456 | Dược |
| **Cashier** | cashier@his.local | 123456 | Thu ngân |

**⚠️ Lưu ý:** Đổi mật khẩu ngay sau khi đăng nhập lần đầu!

---

## 📡 API Documentation

### **Base URL**
```
http://localhost:7239/api/AppointmentsApi
```

### **Endpoints**

#### **1. Lấy Danh Sách Khoa**
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
      "name": "Khoa Khám bệnh"
    }
  ]
}
```

#### **2. Lấy Danh Sách Bác Sĩ**
```http
GET /api/AppointmentsApi/Doctors?departmentId=1
```

**Response:**
```json
{
  "success": true,
  "doctors": [
    {
      "staffId": 1,
      "fullName": "Nguyễn Văn Minh",
      "departmentId": 1,
      "departmentName": "Khoa Khám bệnh"
    }
  ]
}
```

#### **3. Xem Khung Giờ Còn Trống**
```http
GET /api/AppointmentsApi/AvailableSlots?date=2026-01-15&departmentId=1
```

**Response:**
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
      "booked": 3,
      "maxCapacity": 10,
      "available": 7
    }
  ]
}
```

#### **4. Đặt Lịch Khám**
```http
POST /api/AppointmentsApi/Book
Content-Type: application/json

{
  "fullName": "Nguyễn Văn A",
  "phone": "0123456789",
  "dob": "1990-01-01",
  "gender": 1,
  "departmentId": 1,
  "doctorId": 1,
  "date": "2026-01-15",
  "timeSlotId": 1,
  "note": "Khám tổng quát"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Dat lich thanh cong",
  "appointmentCode": "APT20260115123456",
  "appointmentId": 10,
  "date": "15/01/2026"
}
```

#### **5. Tra Cứu Lịch Hẹn**
```http
GET /api/AppointmentsApi/Check?code=APT20260115123456
```

**Response:**
```json
{
  "success": true,
  "appointment": {
    "code": "APT20260115123456",
    "status": 1,
    "date": "15/01/2026",
    "timeSlot": "08:00 - 09:00",
    "patient": {
      "fullName": "Nguyễn Văn A",
      "phone": "0123456789",
      "gender": 1
    },
    "department": "Khoa Khám bệnh",
    "doctor": "Nguyễn Văn Minh",
    "note": "Khám tổng quát"
  }
}
```

### **Error Responses**
```json
{
  "success": false,
  "message": "Loi server"
}
```

**Xem thêm:** [API_INTEGRATION_GUIDE.md](./API_INTEGRATION_GUIDE.md)

---

## 🗄️ Database Schema

### **Core Tables**

#### **Department (Khoa/Phòng ban)**
```sql
DepartmentId (PK)
Code (nvarchar(50))
Name (nvarchar(200))
```

#### **Staff (Nhân viên/Bác sĩ)**
```sql
StaffId (PK)
FullName (nvarchar(200))
DepartmentId (FK)
StaffType (nvarchar(50))  -- DOCTOR, NURSE, etc.
```

#### **Patient (Bệnh nhân)**
```sql
PatientId (PK)
FullName (nvarchar(200))
Dob (date)
Gender (int)  -- 0=Unknown, 1=Male, 2=Female, 3=Other
Phone (nvarchar(20))
```

#### **TimeSlot (Khung giờ)**
```sql
TimeSlotId (PK)
Code (nvarchar(20))
Start (time)
End (time)
```

#### **Appointment (Lịch hẹn)**
```sql
AppointmentId (PK)
Code (nvarchar(30))
PatientId (FK)
DepartmentId (FK)
DoctorId (FK)
Date (date)
TimeSlotId (FK)
Status (int)  -- 1=Booked, 9=Cancelled
Note (nvarchar(500))
CreatedAt (datetime2)
```

#### **Encounter (Lượt khám)**
```sql
EncounterId (PK)
PatientId (FK)
AppointmentId (FK)
DoctorId (FK)
Status (int)  -- 1=CheckedIn, 2=InService, 8=Completed, 9=Cancelled
CheckInAt (datetime2)
EndAt (datetime2)
Diagnosis (nvarchar(500))
Conclusion (nvarchar(1000))
```

#### **Service (Dịch vụ/Xét nghiệm)**
```sql
ServiceId (PK)
Name (nvarchar(200))
Type (nvarchar(50))  -- LAB, IMAGING, etc.
Price (decimal(18,2))
```

#### **Order (Chỉ định)**
```sql
OrderId (PK)
EncounterId (FK)
ServiceId (FK)
Status (int)  -- 1=Requested, 6=Resulted, 7=Verified, 9=Cancelled
OrderedAt (datetime2)
```

#### **OrderResult (Kết quả)**
```sql
OrderResultId (PK)
OrderId (FK)
Result (nvarchar(max))
VerifiedBy (nvarchar(200))
VerifiedAt (datetime2)
```

#### **Invoice (Hóa đơn)**
```sql
InvoiceId (PK)
EncounterId (FK)
InvoiceCode (nvarchar(30))
TotalAmount (decimal(18,2))
Status (int)  -- 1=Unpaid, 8=Paid
Note (nvarchar(500))
PaidAt (datetime2)
PaidBy (nvarchar(200))
CreatedAt (datetime2)
```

### **Entity Relationships**

```
Patient 1---* Appointment *---1 Department
Patient 1---* Encounter   *---1 Doctor (Staff)
Appointment *---1 TimeSlot
Appointment 1---0..1 Encounter
Encounter 1---* Order *---1 Service
Order 1---0..1 OrderResult
Encounter 1---0..1 Invoice
```

---

## 📸 Screenshots

### Landing Page
![Landing Page](docs/screenshots/landing.png)

### Reception - Appointment Management
![Reception](docs/screenshots/reception.png)

### Doctor - Examination
![Doctor](docs/screenshots/doctor.png)

### Lab - Test Results
![Lab](docs/screenshots/lab.png)

### Cashier - Invoice
![Cashier](docs/screenshots/cashier.png)

---

## 🔨 Hướng Dẫn Phát Triển

### **Quy ước MVC (Area)**

| GET | POST (luồng KCB) |
|-----|------------------|
| `Controller` → `{Area}ViewService` → ViewModel → View | `Controller` → `OutpatientWorkflowService` → Redirect |

- Không đặt logic nghiệp vụ dài trong Controller.
- Chuỗi UI: tiếng Việt có dấu, UTF-8.
- Chạy chuẩn hóa chuỗi: `dotnet run --project HisMvc/Scripts/ViConvert/ViConvert.csproj`

### **Thêm Area Mới**

```bash
# Tạo Area mới
dotnet aspnet-codegenerator area Pharmacy

# Tạo Controller
dotnet aspnet-codegenerator controller -name HomeController -area Pharmacy -outDir Areas/Pharmacy/Controllers
```

### **Thêm Migration Mới**

```bash
# Add migration
dotnet ef migrations add AddNewFeature

# Update database
dotnet ef database update

# Rollback
dotnet ef database update PreviousMigration
```

### **Thêm Entity Mới**

1. Tạo class trong `Entities/`
2. Thêm `DbSet<T>` trong `AppDbContext`
3. Add migration và update database
4. Seed data trong `SeedData.cs` (optional)

### **Thêm API Endpoint Mới**

```csharp
// Controllers/Api/YourApiController.cs
[ApiController]
[Route("api/[controller]")]
public class YourApiController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        return Ok(new { success = true, data = "..." });
    }
}
```

### **Testing**

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

---

## 🐛 Troubleshooting

### **1. Lỗi Connection String**
```
❌ Error: Cannot connect to database
```
**Giải pháp:**
- Kiểm tra SQL Server đã chạy chưa
- Verify connection string trong `appsettings.json`
- Test connection bằng SQL Server Management Studio

### **2. Lỗi Migration**
```
❌ Error: Unable to create migration
```
**Giải pháp:**
```bash
# Remove last migration
dotnet ef migrations remove

# Clean solution
dotnet clean

# Rebuild and retry
dotnet build
dotnet ef migrations add YourMigration
```

### **3. Lỗi CORS**
```
❌ Error: No 'Access-Control-Allow-Origin' header
```
**Giải pháp:**
- Thêm domain vào `Portal:AllowedOrigin` trong `appsettings.json`
- Hoặc update CORS policy trong `Program.cs`

### **4. Lỗi 404 Not Found**
```
❌ Error: 404 when accessing /Reception/Home/Index
```
**Giải pháp:**
- Kiểm tra Area routing trong `Program.cs`
- Verify controller có `[Area("Reception")]` attribute
- Clear browser cache và cookies

### **5. Seed Data Không Chạy**
```
❌ Users không được tạo tự động
```
**Giải pháp:**
- Delete database và recreate: `dotnet ef database update`
- Chạy lại application (seed trong `Program.cs`)

### **6. Tiếng Việt / Encoding**
- UI dùng UTF-8; font mặc định: Segoe UI (`wwwroot/css/site.css`)
- Công cụ chuẩn hóa chuỗi: `dotnet run --project HisMvc/Scripts/ViConvert/ViConvert.csproj`
- Không commit `*.log`, `bin/`, `obj/`

---

## 📦 Deployment

### **1. Deploy to IIS**

```bash
# Publish project
dotnet publish -c Release -o ./publish

# Copy files to IIS folder
# Configure IIS Application Pool (.NET CLR Version: No Managed Code)
# Set connection string in appsettings.Production.json
```

### **2. Deploy to Azure**

```bash
# Install Azure CLI
az login

# Create App Service
az webapp up --name your-app-name --runtime "DOTNET|10.0"

# Configure connection string
az webapp config connection-string set --connection-string-type SQLAzure \
  --name your-app-name --resource-group your-rg \
  --settings Default="YOUR_CONNECTION_STRING"
```

### **3. Deploy với Docker**

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["HisMvc/HisMvc.csproj", "HisMvc/"]
RUN dotnet restore "HisMvc/HisMvc.csproj"
COPY . .
WORKDIR "/src/HisMvc"
RUN dotnet build "HisMvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HisMvc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HisMvc.dll"]
```

```bash
# Build and run
docker build -t hismvc .
docker run -d -p 8080:80 --name hismvc hismvc
```

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---