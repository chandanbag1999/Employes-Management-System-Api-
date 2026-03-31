using EMS.Application.Modules.Identity.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EMS.Infrastructure.BackgroundServices;


public class TokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public TokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<TokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repo = scope.ServiceProvider
                    .GetRequiredService<IRefreshTokenRepository>();

                await repo.RemoveExpiredTokensAsync();
                await repo.SaveChangesAsync();

                _logger.LogInformation(
                    "Token cleanup completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token cleanup failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}