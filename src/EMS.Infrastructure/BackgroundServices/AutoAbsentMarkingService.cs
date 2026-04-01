using EMS.Application.Modules.Attendance.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EMS.Infrastructure.BackgroundServices;

public class AutoAbsentMarkingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoAbsentMarkingService> _logger;

    public AutoAbsentMarkingService(
        IServiceScopeFactory scopeFactory,
        ILogger<AutoAbsentMarkingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutoAbsentMarkingService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // IST mein aaj ki date
                var istZone = TimeZoneInfo.FindSystemTimeZoneById(
                    OperatingSystem.IsWindows()
                        ? "India Standard Time"
                        : "Asia/Kolkata");

                var istNow = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow, istZone);

                // Aaj raat 11:00 PM IST pe chalao
                // IST 23:00 = UTC 17:30
                var targetIst = istNow.Date.AddHours(23);

                // Agar already 11 PM ke baad hai — kal chalao
                if (istNow >= targetIst)
                    targetIst = targetIst.AddDays(1);

                var delay = targetIst - istNow;

                _logger.LogInformation(
                    "Next auto-absent run at {TargetTime} IST " +
                    "(in {Minutes} minutes)",
                    targetIst.ToString("dd-MMM-yyyy HH:mm"),
                    (int)delay.TotalMinutes);

                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                await RunAbsentMarkingAsync();
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutoAbsentMarkingService");
                // Error pe 5 min wait karo aur retry karo
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("AutoAbsentMarkingService stopped.");
    }

    private async Task RunAbsentMarkingAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider
            .GetRequiredService<IAttendanceService>();

        var istZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows()
                ? "India Standard Time"
                : "Asia/Kolkata");

        var today = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, istZone).Date;

        // Weekend skip karo
        if (today.DayOfWeek == DayOfWeek.Saturday ||
            today.DayOfWeek == DayOfWeek.Sunday)
        {
            _logger.LogInformation(
                "Skipping auto-absent for {Date} (Weekend)", today);
            return;
        }

        _logger.LogInformation(
            "Running auto-absent marking for {Date}", today);

        var count = await service.MarkAbsentForDateAsync(today);

        _logger.LogInformation(
            "Auto-absent marking complete. {Count} employees marked absent.",
            count);
    }
}