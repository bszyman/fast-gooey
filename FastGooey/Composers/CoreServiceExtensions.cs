using FastGooey.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FastGooey.Composers;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IKeyValueService, KeyValueService>();
        services.AddSingleton<IAppleSignInJwtService, AppleSignInJwtService>();
        services.AddTransient<IEmailSender, EmailerService>();
        services.AddTransient<EmailerService>();

        return services;
    }
}