using System.Security.Claims;
using HisMvc.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services;

/// <summary>
/// Lấy StaffId từ tài khoản đăng nhập (AspNetUsers.StaffId), không tim theo email = FullName.
/// </summary>
public class CurrentStaffService
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public CurrentStaffService(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<int?> TryGetStaffIdAsync(ClaimsPrincipal user)
    {
        var appUser = await _userManager.GetUserAsync(user);
        if (appUser?.StaffId is int sid && sid > 0)
            return sid;

        var email = user.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
            return null;

        appUser = await _userManager.FindByEmailAsync(email);
        if (appUser?.StaffId is int linked && linked > 0)
            return linked;

        return null;
    }

    public async Task<int> GetStaffIdAsync(ClaimsPrincipal user, int? fallbackStaffId = null)
    {
        var id = await TryGetStaffIdAsync(user);
        if (id.HasValue)
            return id.Value;

        if (fallbackStaffId is int fb && fb > 0)
            return fb;

        throw new InvalidOperationException(
            "Tai khoan chua lien ket nhân viên (StaffId). Vao Admin > Tai khoan de gan nhân viên.");
    }

    // Tien dung: tra ve null neu không có (cho cac truong hop optional)
    public Task<int?> GetCurrentStaffIdAsync(ClaimsPrincipal user) => TryGetStaffIdAsync(user);
}
