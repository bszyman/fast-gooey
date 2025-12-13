using FastGooey.Models.WebServiceResponse;
using Flurl.Http;

namespace FastGooey.Services;

public interface ITurnstileValidatorService
{
    public Task<bool> ValidateFormRequest(string? token);
}

public class TurnstileValidatorService(
    IConfiguration configuration, 
    IHttpContextAccessorService httpContextAccessorService, 
    IWebHostEnvironment environment,
    ILogger<TurnstileValidatorService> logger): 
    ITurnstileValidatorService
{
    public async Task<bool> ValidateFormRequest(string? token)
    {
        if (!environment.IsProduction())
        {
            return true;
        }
        
        var cloudflareEnabled = configuration.GetValue<bool?>("CloudFlare:enabled");

        if (!cloudflareEnabled.HasValue)
        {
            throw new Exception("CloudFlare__enabled setting is not configured");
        }

        if (!cloudflareEnabled.Value)
        {
            return true;
        }

        if (string.IsNullOrEmpty(token) && cloudflareEnabled.Value)
        {
            return false;
        }

        var turnstileToken = configuration.GetValue<string?>("CloudFlare:turnstileKey");

        if (string.IsNullOrEmpty(turnstileToken))
        {
            throw new Exception("CloudFlare__turnstileKey setting is not configured");
        }

        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        try
        {
            var remoteIpAddress = httpContextAccessorService.GetRemoteIpAddress();

            var response = await "https://challenges.cloudflare.com/turnstile/v0/siteverify"
                .PostUrlEncodedAsync(new
                {
                    secret = turnstileToken,
                    response = token,
                    remoteip = remoteIpAddress
                });

            var result = await response.GetJsonAsync<TurnstileResponse>();

            return result.Success;
        }
        catch (FlurlHttpException ex)
        {
            logger.LogError("Error validating CloudFlare turnstile token: {Message}", ex.Message);
            return false;
        }
    }
}