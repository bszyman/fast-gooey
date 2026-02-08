using FastGooey.Services;
using FastGooey.Services.Media;
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
        services.AddSingleton<IMediaCredentialProtector, MediaCredentialProtector>();
        services.AddScoped<IMediaSourceProvider, S3MediaSourceProvider>();
        services.AddScoped<IMediaSourceProvider, AzureBlobMediaSourceProvider>();
        services.AddScoped<IMediaSourceProvider, WebDavMediaSourceProvider>();
        services.AddScoped<IMediaSourceProviderRegistry, MediaSourceProviderRegistry>();
        services.AddHttpClient();

        return services;
    }
}
