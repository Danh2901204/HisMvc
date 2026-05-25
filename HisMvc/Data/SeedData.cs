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
                new Department { Code = "KB", Name = "Khoa Kham benh" },
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

        // 5) Seed Doctors (Staff)
        if (!await db.Staffs.AnyAsync())
        {
            logger.LogInformation("Seeding staff...");
            var kb = await db.Departments.FirstAsync(x => x.Code == "KB");
            var tmh = await db.Departments.FirstAsync(x => x.Code == "TMH");

            db.Staffs.AddRange(
                new Staff { FullName = "Nguyen Van Minh", DepartmentId = kb.DepartmentId, StaffType = "DOCTOR" },
                new Staff { FullName = "Tran Thi Huong", DepartmentId = kb.DepartmentId, StaffType = "DOCTOR" },
                new Staff { FullName = "Pham Duc Long", DepartmentId = tmh.DepartmentId, StaffType = "DOCTOR" }
            );
            await db.SaveChangesAsync();
        }
        
        // 6) Seed Services
        if (!await db.Services.AnyAsync())
        {
            logger.LogInformation("Seeding services...");
            db.Services.AddRange(
                new Service { Name = "Cong thuc mau (CBC)", Type = "LAB", Price = 80000 },
                new Service { Name = "Duong huyet", Type = "LAB", Price = 50000 },
                new Service { Name = "X-quang nguc thang", Type = "IMAGING", Price = 120000 }
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

        logger.LogInformation("Database seeding completed successfully.");
    }

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
}
