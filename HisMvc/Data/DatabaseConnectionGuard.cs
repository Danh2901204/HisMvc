using Microsoft.EntityFrameworkCore;

namespace HisMvc.Data;

/// <summary>
/// Dam bao ung dung chi dung SQL Server (khong LocalDB) va phai ket noi duoc moi có dữ liệu.
/// </summary>
public static class DatabaseConnectionGuard
{
    public const string RequiredServerHint = "localhost";

    public static string RequireSqlServerConnectionString(IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException(
                "Thieu ConnectionStrings:Default. Ung dung bat buoc ket noi SQL Server.");

        if (cs.Contains("(localdb)", StringComparison.OrdinalIgnoreCase) ||
            cs.Contains("mssqllocaldb", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Không cho phep LocalDB. Hay cau hinh SQL Server, vi du: " +
                "Server=localhost;Database=HIS_MVC_DB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=True");

        return cs;
    }

    public static async Task EnsureCanConnectAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        try
        {
            if (!await db.Database.CanConnectAsync(ct))
            {
                throw new InvalidOperationException(
                    "Không ket noi duoc SQL Server. Kiểm tra dịch vụ SQL Server (MSSQLSERVER) dang chay va database HIS_MVC_DB tồn tại.");
            }

            logger.LogInformation(
                "SQL Server connected. Database: {Database}",
                db.Database.GetDbConnection().Database);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger.LogCritical(ex, "Lỗi ket noi SQL Server.");
            throw new InvalidOperationException(
                "Không ket noi duoc SQL Server. Ung dung không thể luu hoặc doc du lieu.", ex);
        }
    }
}
