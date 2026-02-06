using FastGooey.BackgroundJobs;
using FastGooey.Services;

namespace FastGooey.Composers;

public static class AppleWeatherKitServiceExtensions
{
    public static IServiceCollection AddAppleWeatherKitServices(this IServiceCollection services)
    {
        services.AddScoped<IAppleWeatherKitJwtService, AppleWeatherKitJwtService>();
        services.AddHostedService<AppleWeatherKitJwtRefreshService>();

        return services;
    }
}
