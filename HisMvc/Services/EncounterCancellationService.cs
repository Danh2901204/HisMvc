using HisMvc.Data;
using HisMvc.Entities;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services;

public interface IEncounterCancellationService
{
    Task<int> CancelStaleActiveEncountersAsync(CancellationToken ct = default);
}

/// <summary>
/// Hủy lượt khám ngoại trú quá ngày nhưng vẫn còn trạng thái đang xử lý.
/// </summary>
public class EncounterCancellationService : IEncounterCancellationService
{
    private static readonly EncounterStatus[] StaleStatuses =
    [
        EncounterStatus.CheckedIn,
        EncounterStatus.WaitingExam,
        EncounterStatus.InService,
        EncounterStatus.WaitingResult,
        EncounterStatus.WaitingFinalPayment
    ];

    private readonly AppDbContext _db;
    private readonly ILogger<EncounterCancellationService> _logger;

    public EncounterCancellationService(AppDbContext db, ILogger<EncounterCancellationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> CancelStaleActiveEncountersAsync(CancellationToken ct = default)
    {
        var (startUtc, _) = EncounterDayHelper.GetLocalTodayUtcRange();
        var now = DateTime.UtcNow;

        var stale = await _db.Encounters
            .Where(e => e.CheckInAt < startUtc && StaleStatuses.Contains(e.Status))
            .ToListAsync(ct);

        if (stale.Count == 0)
            return 0;

        foreach (var enc in stale)
        {
            enc.Status = EncounterStatus.Cancelled;
            enc.EndAt = now;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Cancelled {Count} stale encounter(s) from before today", stale.Count);
        return stale.Count;
    }
}
