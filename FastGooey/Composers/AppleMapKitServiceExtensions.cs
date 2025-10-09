using FastGooey.BackgroundJobs;
using FastGooey.Services;

namespace FastGooey.Composers;

public static class AppleMapKitServiceExtensions
{
    public static IServiceCollection AddAppleMapKitServices(this IServiceCollection services)
    {
        // Register the JWT service as Singleton since it maintains state
        services.AddScoped<IAppleMapKitJwtService, AppleMapKitJwtService>();

        // Register the background service
        services.AddHostedService<AppleMapKitJwtRefreshService>();

        return services;
    }
}