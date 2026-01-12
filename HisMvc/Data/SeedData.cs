using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = sp.GetRequiredService<UserManager<AppUser>>();

        // 1) Seed Roles
        string[] roles =
        {
            AppRoles.ADMIN, AppRoles.RECEPTION, AppRoles.DOCTOR, AppRoles.LAB_TECH,
            AppRoles.PHARMACIST, AppRoles.CASHIER, AppRoles.MANAGER
        };

        foreach (var r in roles)
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new IdentityRole(r));

        // 2) Seed Users (admin + reception + doctor + lab + cashier)
        await EnsureUser(userMgr, "admin@his.local", "123456", AppRoles.ADMIN);
        await EnsureUser(userMgr, "reception@his.local", "123456", AppRoles.RECEPTION);
        await EnsureUser(userMgr, "doctor@his.local", "123456", AppRoles.DOCTOR);
        await EnsureUser(userMgr, "lab@his.local", "123456", AppRoles.LAB_TECH);
        await EnsureUser(userMgr, "cashier@his.local", "123456", AppRoles.CASHIER);


        // 3) Seed Departments
        if (!await db.Departments.AnyAsync())
        {
            db.Departments.AddRange(
                new Department { Code = "KB", Name = "Khoa Khám bệnh" },
                new Department { Code = "NOI", Name = "Nội tổng hợp" },
                new Department { Code = "TMH", Name = "Tai Mũi Họng" }
            );
            await db.SaveChangesAsync();
        }

        // 4) Seed TimeSlots
        if (!await db.TimeSlots.AnyAsync())
        {
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
            var kb = await db.Departments.FirstAsync(x => x.Code == "KB");
            var tmh = await db.Departments.FirstAsync(x => x.Code == "TMH");

            db.Staffs.AddRange(
                new Staff { FullName = "Nguyễn Văn Minh", DepartmentId = kb.DepartmentId, StaffType = "DOCTOR" },
                new Staff { FullName = "Trần Thị Hương", DepartmentId = kb.DepartmentId, StaffType = "DOCTOR" },
                new Staff { FullName = "Phạm Đức Long", DepartmentId = tmh.DepartmentId, StaffType = "DOCTOR" }
            );
            await db.SaveChangesAsync();
        }
        
        // 6) Seed Services
        if (!await db.Services.AnyAsync())
        {
            db.Services.AddRange(
                new Service { Name = "Công thức máu (CBC)", Type = "LAB", Price = 80000 },
                new Service { Name = "Đường huyết", Type = "LAB", Price = 50000 },
                new Service { Name = "X-quang ngực thẳng", Type = "IMAGING", Price = 120000 }
            );
            await db.SaveChangesAsync();
        }

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
