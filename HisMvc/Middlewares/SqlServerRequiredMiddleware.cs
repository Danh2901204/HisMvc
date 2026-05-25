using HisMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Middlewares;

/// <summary>
/// Khi SQL Server mat ket noi trong lúc chay, tra 503 — không phuc vu du lieu.
/// </summary>
public class SqlServerRequiredMiddleware
{
    private readonly RequestDelegate _next;
    private static DateTime _lastCheckUtc = DateTime.MinValue;
    private static bool _lastCheckOk;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(15);
    private static readonly object CheckLock = new();

    public SqlServerRequiredMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (!await IsDatabaseAvailableAsync(db, context.RequestAborted))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(
                "<h1>503 - Không ket noi SQL Server</h1>" +
                "<p>Hệ thống bat buoc luu du lieu tren SQL Server (localhost / HIS_MVC_DB).</p>" +
                "<p>Kiểm tra dịch vụ SQL Server dang chay va thu lai.</p>");
            return;
        }

        await _next(context);
    }

    private static async Task<bool> IsDatabaseAvailableAsync(AppDbContext db, CancellationToken ct)
    {
        lock (CheckLock)
        {
            if (DateTime.UtcNow - _lastCheckUtc < CheckInterval)
                return _lastCheckOk;
        }

        var ok = false;
        try
        {
            ok = await db.Database.CanConnectAsync(ct);
        }
        catch
        {
            ok = false;
        }

        lock (CheckLock)
        {
            _lastCheckUtc = DateTime.UtcNow;
            _lastCheckOk = ok;
        }

        return ok;
    }
}
