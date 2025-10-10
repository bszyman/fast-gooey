using FastGooey.BackgroundJobs;
using FastGooey.Services;

namespace FastGooey.BackgroundJobs;
public class AppleMapKitJwtRefreshService : BackgroundService
{
    private readonly ILogger<AppleMapKitJwtRefreshService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(20);

    public AppleMapKitJwtRefreshService(
        ILogger<AppleMapKitJwtRefreshService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Apple MapKit JWT Refresh Service is starting.");

        using var timer = new PeriodicTimer(_refreshInterval);

        // Do initial refresh
        await RefreshJwtAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested && 
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RefreshJwtAsync(stoppingToken);
        }
    }

    private async Task RefreshJwtAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Refreshing Apple MapKit JWT at: {time}", DateTimeOffset.UtcNow);
            
            using var scope = _serviceProvider.CreateScope();
            var jwtService = scope.ServiceProvider.GetRequiredService<IAppleMapKitJwtService>();
            await jwtService.RefreshTokenAsync(stoppingToken);
            
            _logger.LogInformation("Apple MapKit JWT refreshed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while refreshing Apple MapKit JWT.");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Apple MapKit JWT Refresh Service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}