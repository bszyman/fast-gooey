using FastGooey.Services;

namespace FastGooey.Composers;

public static class CloudflareServicesComposer
{
    public static IServiceCollection AddCloudflareTurnstileServices(this IServiceCollection services)
    {
        
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IHttpContextAccessorService, HttpContextAccessorService>();
        services.AddSingleton<ITurnstileValidatorService, TurnstileValidatorService>();

        return services;
    }
}