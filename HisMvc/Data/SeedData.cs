using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HisMvc.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<AppDbContext>();
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("SeedData");
        
        try
        {
            logger.LogInformation("Starting database migration...");
            await db.Database.MigrateAsync();
            logger.LogInformation("Database migration completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database migration.");
            throw;
        }

        var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = sp.GetRequiredService<UserManager<AppUser>>();

        // 1) Seed Roles
        logger.LogInformation("Seeding roles...");
        string[] roles =
        {
            AppRoles.ADMIN, AppRoles.RECEPTION, AppRoles.DOCTOR, AppRoles.LAB_TECH,
            AppRoles.PHARMACIST, AppRoles.CASHIER, AppRoles.MANAGER
        };

        foreach (var r in roles)
            if (!await roleMgr.RoleExistsAsync(r))
            {
                logger.LogInformation("Creating role: {Role}", r);
                await roleMgr.CreateAsync(new IdentityRole(r));
            }

        // 2) Seed Users (admin + reception + doctor + lab + cashier + pharmacist)
        logger.LogInformation("Seeding users...");
        await EnsureUser(userMgr, "admin@his.local", "123456", AppRoles.ADMIN);
        await EnsureUser(userMgr, "reception@his.local", "123456", AppRoles.RECEPTION);
        await EnsureUser(userMgr, "doctor@his.local", "123456", AppRoles.DOCTOR);
        await EnsureUser(userMgr, "lab@his.local", "123456", AppRoles.LAB_TECH);
        await EnsureUser(userMgr, "cashier@his.local", "123456", AppRoles.CASHIER);
        await EnsureUser(userMgr, "pharmacist@his.local", "123456", AppRoles.PHARMACIST);



        // 3) Seed Departments
        if (!await db.Departments.AnyAsync())
        {
            logger.LogInformation("Seeding departments...");
            db.Departments.AddRange(
                new Department { Code = "KB", Name = "Khoa Khám bệnh" },
                new Department { Code = "NOI", Name = "Noi tong hop" },
                new Department { Code = "TMH", Name = "Tai Mui Hong" }
            );
            await db.SaveChangesAsync();
        }

        // 4) Seed TimeSlots
        if (!await db.TimeSlots.AnyAsync())
        {
            logger.LogInformation("Seeding time slots...");
            db.TimeSlots.AddRange(
                new TimeSlot { Code = "S1", Start = new TimeOnly(8, 0), End = new TimeOnly(9, 0) },
                new TimeSlot { Code = "S2", Start = new TimeOnly(9, 0), End = new TimeOnly(10, 0) },
                new TimeSlot { Code = "S3", Start = new TimeOnly(10, 0), End = new TimeOnly(11, 0) },
                new TimeSlot { Code = "C1", Start = new TimeOnly(13, 0), End = new TimeOnly(14, 0) }
            );
            await db.SaveChangesAsync();
        }

        // 5) Seed Staff (bac si + cac vai tro khac theo Book1 / AspNetUsers.StaffId)
        var kbDept = await db.Departments.FirstOrDefaultAsync(x => x.Code == "KB")
            ?? await db.Departments.FirstAsync();
        await EnsureStaffAsync(db, "DOCTOR", "Nguyen Van Minh", kbDept.DepartmentId);
        await EnsureStaffAsync(db, "DOCTOR", "Tran Thi Huong", kbDept.DepartmentId);
        var tmhDept = await db.Departments.FirstOrDefaultAsync(x => x.Code == "TMH") ?? kbDept;
        await EnsureStaffAsync(db, "DOCTOR", "Pham Duc Long", tmhDept.DepartmentId);
        await EnsureStaffAsync(db, "RECEPTION", "Le Van Tiep Tan", kbDept.DepartmentId);
        await EnsureStaffAsync(db, "LAB_TECH", "Hoang Lab Ky Thuat", kbDept.DepartmentId);
        await EnsureStaffAsync(db, "PHARMACIST", "Vo Duoc Si Demo", kbDept.DepartmentId);
        await EnsureStaffAsync(db, "CASHIER", "Đang Thu ngân Demo", kbDept.DepartmentId);
        await EnsureStaffAsync(db, "ADMIN", "Admin He Thong", kbDept.DepartmentId);

        // 5b) Gan StaffId cho tài khoản (theo AspNetUsers.StaffId trong Book1)
        await LinkUserStaffAsync(userMgr, db, "doctor@his.local", "DOCTOR");
        await LinkUserStaffAsync(userMgr, db, "reception@his.local", "RECEPTION");
        await LinkUserStaffAsync(userMgr, db, "lab@his.local", "LAB_TECH");
        await LinkUserStaffAsync(userMgr, db, "cashier@his.local", "CASHIER");
        await LinkUserStaffAsync(userMgr, db, "pharmacist@his.local", "PHARMACIST");
        await LinkUserStaffAsync(userMgr, db, "admin@his.local", "ADMIN");
        
        // 6) Seed Services
        if (!await db.Services.AnyAsync())
        {
            logger.LogInformation("Seeding services...");
            db.Services.AddRange(
                new Service { Name = "Cong thuc mau (CBC)", Type = "LAB", Price = 80000 },
                new Service { Name = "Duong huyet", Type = "LAB", Price = 50000 },
                new Service { Name = "X-quang nguc tháng", Type = "IMAGING", Price = 120000 }
            );
            await db.SaveChangesAsync();
        }

        // 7) Seed Medicines (Thuốc mẫu)
        if (!await db.Medicines.AnyAsync())
        {
            logger.LogInformation("Seeding medicines...");

            var medicines = new[]
            {
                new Medicine 
                { 
                    Code = "PAR500", 
                    Name = "Paracetamol 500mg", 
                    ActiveIngredient = "Paracetamol", 
                    Unit = "Vien",
                    Manufacturer = "DHG Pharma",
                    RequiresPrescription = false,
                    Description = "Giam dau, ha sot"
                },
                new Medicine 
                { 
                    Code = "AMX500", 
                    Name = "Amoxicillin 500mg", 
                    ActiveIngredient = "Amoxicillin", 
                    Unit = "Viên",
                    Manufacturer = "Imexpharm",
                    RequiresPrescription = true,
                    Description = "Kháng sinh điều trị nhiễm khuẩn"
                },
                new Medicine 
                { 
                    Code = "VIT-C", 
                    Name = "Vitamin C 500mg", 
                    ActiveIngredient = "Acid Ascorbic", 
                    Unit = "Viên",
                    Manufacturer = "TPBVSK",
                    RequiresPrescription = false,
                    Description = "Bổ sung vitamin C"
                },
                new Medicine 
                { 
                    Code = "OMP20", 
                    Name = "Omeprazole 20mg", 
                    ActiveIngredient = "Omeprazole", 
                    Unit = "Viên",
                    Manufacturer = "Boston",
                    RequiresPrescription = true,
                    Description = "Điều trị loét dạ dày, trào ngược"
                }
            };

            db.Medicines.AddRange(medicines);
            await db.SaveChangesAsync();

            // Thêm lô thuốc mẫu cho mỗi loại
            foreach (var med in medicines)
            {
                db.MedicineBatches.Add(new MedicineBatch
                {
                    MedicineId = med.MedicineId,
                    BatchNumber = $"LOT-{med.Code}-001",
                    ManufactureDate = DateTime.Today.AddMonths(-6),
                    ExpiryDate = DateTime.Today.AddYears(2),
                    UnitPrice = 1000,
                    QuantityInStock = 1000,
                    MinStockLevel = 50,
                    IsActive = true
                });
            }
            await db.SaveChangesAsync();
        }


        // 8) Seed Sample Patients with Insurance (Bệnh nhân mẫu có BHYT)
        if (!await db.Patients.AnyAsync())
        {
            logger.LogInformation("Seeding patients...");
            db.Patients.AddRange(
                new Patient
                {
                    FullName = "Nguyễn Văn An",
                    Dob = new DateOnly(1990, 5, 15),
                    Gender = Gender.Male,
                    Phone = "0901234567",
                    Address = "123 Đường ABC, Quận 1, TP.HCM",
                    IdentityNumber = "001090012345",
                    InsuranceNumber = "DN1234567890123",
                    InsuranceExpiry = DateTime.Today.AddMonths(6),
                    InsuranceType = "KC",
                    InsuranceCoveragePercent = 80,
                    InsuranceHospital = "BV Đa khoa Tỉnh"
                },
                new Patient
                {
                    FullName = "Trần Thị Bình",
                    Dob = new DateOnly(1985, 8, 20),
                    Gender = Gender.Female,
                    Phone = "0912345678",
                    Address = "456 Đường XYZ, Quận 2, TP.HCM",
                    IdentityNumber = "001085012345",
                    InsuranceNumber = "DN9876543210987",
                    InsuranceExpiry = DateTime.Today.AddYears(1),
                    InsuranceType = "QN",
                    InsuranceCoveragePercent = 100,
                    InsuranceHospital = "BV Quân Y 175"
                },
                new Patient
                {
                    FullName = "Lê Văn Cường",
                    Dob = new DateOnly(1995, 3, 10),
                    Gender = Gender.Male,
                    Phone = "0923456789",
                    Address = "789 Đường DEF, Quận 3, TP.HCM",
                    // Không có BHYT
                }
            );
            await db.SaveChangesAsync();
        }

        // 9) Seed Wards & Beds (Buồng bệnh nội trú)
        if (!await db.Wards.AnyAsync())
        {
            logger.LogInformation("Seeding wards and beds...");

            var noiKhoa = await db.Departments.FirstAsync(x => x.Code == "NOI");

            var wardGeneral = new Ward
            {
                Code = "NOI-A",
                Name = "Buồng Nội Tổng Hợp A",
                DepartmentId = noiKhoa.DepartmentId,
                Type = WardType.General,
                TotalBeds = 10,
                IsActive = true,
                Description = "Buồng bệnh thường"
            };

            var wardVIP = new Ward
            {
                Code = "VIP-1",
                Name = "Buồng VIP 1",
                DepartmentId = noiKhoa.DepartmentId,
                Type = WardType.VIP,
                TotalBeds = 5,
                IsActive = true,
                Description = "Buồng bệnh VIP"
            };

            db.Wards.AddRange(wardGeneral, wardVIP);
            await db.SaveChangesAsync();

            // Thêm giường cho buồng thường
            for (int i = 1; i <= 10; i++)
            {
                db.Beds.Add(new Bed
                {
                    WardId = wardGeneral.WardId,
                    BedNumber = $"A{i:D2}",
                    Status = BedStatus.Empty,
                    IsActive = true
                });
            }

            // Thêm giường cho buồng VIP
            for (int i = 1; i <= 5; i++)
            {
                db.Beds.Add(new Bed
                {
                    WardId = wardVIP.WardId,
                    BedNumber = $"V{i:D2}",
                    Status = BedStatus.Empty,
                    IsActive = true
                });
            }

            await db.SaveChangesAsync();
        }

        // 10) Cấu hình BHYT (InsuranceConfigs - Book1)
        if (!await db.InsuranceConfigs.AnyAsync())
        {
            logger.LogInformation("Seeding insurance configs...");
            db.InsuranceConfigs.AddRange(
                new InsuranceConfig { InsuranceType = "QN", Description = "Quan nhan", DefaultCoveragePercent = 100, RequireRegistration = true, IsActive = true },
                new InsuranceConfig { InsuranceType = "KC", Description = "Kham chua benh", DefaultCoveragePercent = 80, RequireRegistration = true, IsActive = true },
                new InsuranceConfig { InsuranceType = "GD", Description = "Gia dinh", DefaultCoveragePercent = 80, RequireRegistration = true, IsActive = true },
                new InsuranceConfig { InsuranceType = "HT", Description = "Ho ngheo / can ngheo", DefaultCoveragePercent = 95, RequireRegistration = true, IsActive = true }
            );
            await db.SaveChangesAsync();
        }

        // 11) Danh muc ICD-10 thuong gap (ngoai tru)
        if (!await db.Icd10Catalogs.AnyAsync())
        {
            logger.LogInformation("Seeding ICD-10 catalog...");
            db.Icd10Catalogs.AddRange(GetCommonIcd10Codes());
            await db.SaveChangesAsync();
        }

        logger.LogInformation("Database seeding completed successfully.");
    }

    private static IEnumerable<Icd10Catalog> GetCommonIcd10Codes() => new[]
    {
        Icd("J00", "Cam cum", "Ho hap"),
        Icd("J06.9", "Viem duong ho hap cap, không xac dinh vi sinh vat", "Ho hap"),
        Icd("J02", "Viem hong cap", "Ho hap"),
        Icd("J03", "Viem amidan cap", "Ho hap"),
        Icd("J20", "Viêm phế quản cấp", "Ho hap"),
        Icd("J18", "Viêm phổi, không xác định tác nhân", "Ho hap"),
        Icd("J45", "Hen phe quan", "Ho hap"),
        Icd("R05", "Ho", "Trieu chung"),
        Icd("R50", "Sot, không xac dinh", "Trieu chung"),
        Icd("R51", "Dau dau", "Trieu chung"),
        Icd("R10", "Dau bung va vung bung", "Trieu chung"),
        Icd("R11", "Buon non va non", "Trieu chung"),
        Icd("I10", "Tang huyet ap man tinh", "Tim mach"),
        Icd("I25", "Benh tim thieu mau man", "Tim mach"),
        Icd("I50", "Suy tim", "Tim mach"),
        Icd("I48", "Rung nhi", "Tim mach"),
        Icd("E11", "Dai thao duong type 2", "Noi tiet"),
        Icd("E11.9", "Dai thao duong type 2, không bien chung", "Noi tiet"),
        Icd("E78", "Roi loan lipoprotein va tang lipid mau", "Noi tiet"),
        Icd("K29.5", "Viêm da day mãn tính, không xác định", "Tieu hoa"),
        Icd("K30", "Roi loan tieu hoa, không xac dinh", "Tieu hoa"),
        Icd("K21", "Benh trao nguoc da day-thuc quan", "Tieu hoa"),
        Icd("K59.0", "Tạo bon", "Tieu hoa"),
        Icd("K92.2", "Xuat huyet tieu hoa, không xac dinh", "Tieu hoa"),
        Icd("A09", "Tieu chay va viem duong ruot nhiem trung", "Nhiem trung"),
        Icd("B34.9", "Nhiem virus, không xac dinh", "Nhiem trung"),
        Icd("M54", "Dau lung", "Co xuong"),
        Icd("M54.5", "Dau vung that lung duoi", "Co xuong"),
        Icd("M79.3", "Dau co, không xac dinh", "Co xuong"),
        Icd("M25.5", "Dau khop", "Co xuong"),
        Icd("H10", "Viem ket mac", "Mat"),
        Icd("H66", "Viem tai giua", "Tai mui hong"),
        Icd("H81", "Roi loan chuc nang tieu cau", "Tai mui hong"),
        Icd("N39", "Roi loan he tiet nieu, không xac dinh", "Tiet nieu"),
        Icd("N30", "Viem bang quang", "Tiet nieu"),
        Icd("L30", "Viêm da, không xác định", "Đã lieu"),
        Icd("L20", "Viêm da di ung", "Đã lieu"),
        Icd("F32", "Dot xuất tram cam", "Tam than"),
        Icd("F41", "Roi loan lo au khac", "Tam than"),
        Icd("G43", "Migraine", "Than kinh"),
        Icd("G44", "Dau dau nguyen phat khac", "Than kinh"),
        Icd("D50", "Thieu mau do thieu sat", "Mau"),
        Icd("E03", "Suy giap khac", "Noi tiet"),
        Icd("J30", "Viêm mũi dị ứng", "Ho hap"),
        Icd("Z00.0", "Kham suc khoe tong quat", "Kham"),
        Icd("Z01.8", "Kham chuyen khoa khac", "Kham"),
    };

    private static Icd10Catalog Icd(string code, string name, string chapter) =>
        new() { Code = code, Name = name, Chapter = chapter, IsCommon = true };

    private static async Task EnsureUser(UserManager<AppUser> userMgr, string email, string password, string role)
    {
        var user = await userMgr.FindByEmailAsync(email);
        if (user == null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            await userMgr.CreateAsync(user, password);
        }

        if (!await userMgr.IsInRoleAsync(user, role))
            await userMgr.AddToRoleAsync(user, role);
    }

    private static async Task LinkUserStaffAsync(
        UserManager<AppUser> userMgr,
        AppDbContext db,
        string email,
        string staffType)
    {
        var user = await userMgr.FindByEmailAsync(email);
        if (user == null || user.StaffId.HasValue)
            return;

        var staff = await db.Staffs.FirstOrDefaultAsync(s => s.StaffType == staffType);
        if (staff == null && staffType == "ADMIN")
            staff = await db.Staffs.FirstOrDefaultAsync(s => s.StaffType == "ADMIN");
        staff ??= await db.Staffs.FirstOrDefaultAsync();

        if (staff == null)
            return;

        user.StaffId = staff.StaffId;
        await userMgr.UpdateAsync(user);
    }

    private static async Task EnsureStaffAsync(AppDbContext db, string staffType, string fullName, int departmentId)
    {
        if (await db.Staffs.AnyAsync(s => s.StaffType == staffType && s.FullName == fullName))
            return;

        if (staffType is "RECEPTION" or "LAB_TECH" or "PHARMACIST" or "CASHIER" or "ADMIN")
        {
            if (await db.Staffs.AnyAsync(s => s.StaffType == staffType))
                return;
        }

        db.Staffs.Add(new Staff
        {
            FullName = fullName,
            DepartmentId = departmentId,
            StaffType = staffType
        });
        await db.SaveChangesAsync();
    }
}
