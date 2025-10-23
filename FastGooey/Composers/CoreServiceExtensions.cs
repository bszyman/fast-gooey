using FastGooey.Services;

namespace FastGooey.Composers;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    { 
        services.AddScoped<IKeyValueService, KeyValueService>();
        services.AddSingleton<IAppleSignInJwtService, AppleSignInJwtService>();

        return services;
    }
}