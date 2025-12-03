using FastGooey.Services;

namespace FastGooey.Composers;

public static class CloudflareServicesComposer
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<ITurnstileValidatorService, TurnstileValidatorService>();

        return services;
    }
}