namespace HisMvc.Services;

/// <summary>
/// Chạy ngầm quét lịch hẹn quá hạn 60 phút chưa check-in → chuyển NoShow.
/// </summary>
public class AppointmentCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AppointmentCleanupService> _logger;
    private readonly TimeSpan _interval;

    public AppointmentCleanupService(
        IServiceProvider services,
        ILogger<AppointmentCleanupService> logger,
        IConfiguration configuration)
    {
        _services = services;
        _logger = logger;
        var minutes = configuration.GetValue("Appointments:CleanupIntervalMinutes", 5);
        _interval = TimeSpan.FromMinutes(Math.Max(1, minutes));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "AppointmentCleanupService started (interval {Interval}, NoShow grace {Grace} min)",
            _interval,
            AppointmentCancellationService.NoShowGraceMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Appointment cleanup cycle failed");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var cancellation = scope.ServiceProvider.GetRequiredService<IAppointmentCancellationService>();
        var marked = await cancellation.MarkOverdueAsNoShowAsync(ct);

        if (marked > 0)
            _logger.LogInformation("Marked {Count} appointment(s) as NoShow", marked);
    }
}
