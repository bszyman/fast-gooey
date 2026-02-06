using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using FastGooey.Models.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FastGooey.Services;

public interface IAppleWeatherKitJwtService
{
    Task RefreshTokenAsync(CancellationToken cancellationToken = default);
}

public class AppleWeatherKitJwtService(
    ILogger<AppleWeatherKitJwtService> logger,
    IConfiguration configuration,
    IKeyValueService keyValueService) : IAppleWeatherKitJwtService
{
    private const string DefaultWeatherKitOrigin = "https://weatherkit.apple.com";
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await GenerateAndSaveToken(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task GenerateAndSaveToken(CancellationToken cancellationToken = default)
    {
        var config = configuration.GetSection("uCupertino:weatherkit").Get<WeatherKitConfigurationModel>();

        if (config == null)
        {
            logger.LogError("WeatherKit configuration not found");
            return;
        }

        if (!config.Enabled)
        {
            logger.LogInformation("WeatherKit is disabled; skipping JWT refresh.");
            return;
        }

        if (string.IsNullOrEmpty(config.KeyId) ||
            string.IsNullOrEmpty(config.TeamId) ||
            string.IsNullOrEmpty(config.KeyLocation))
        {
            logger.LogError("WeatherKit configuration is incomplete. Required: KeyId, TeamId, KeyLocation");
            return;
        }

        try
        {
            var token = GenerateJwtToken(config);

            await keyValueService.SetValueForKey(Constants.WeatherKitJwt, token);
            await keyValueService.SetValueForKey(Constants.WeatherKitLastRefreshKey, DateTime.UtcNow.ToString("O"));

            logger.LogInformation("WeatherKit JWT token refreshed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating Apple WeatherKit JWT");
            throw;
        }
    }

    private string GenerateJwtToken(WeatherKitConfigurationModel config)
    {
        var privateKeyBytes = LoadPrivateKey(config.KeyLocation!);

        var ecdsa = ECDsa.Create();
        ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

        var securityKey = new ECDsaSecurityKey(ecdsa) { KeyId = config.KeyId };
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddHours(6);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = config.TeamId,
            Audience = string.IsNullOrWhiteSpace(config.Origin) ? DefaultWeatherKitOrigin : config.Origin,
            Claims = new Dictionary<string, object>
            {
                { "sub", config.Origin }
            },
            NotBefore = issuedAt,
            IssuedAt = issuedAt,
            Expires = expiresAt,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        securityToken.Header["typ"] = "JWT";
        securityToken.Header["kid"] = config.KeyId;
        securityToken.Header["id"] = $"{config.TeamId}.${config.Origin}";

        return tokenHandler.WriteToken(securityToken);
    }

    private byte[] LoadPrivateKey(string keyLocation)
    {
        if (!File.Exists(keyLocation))
        {
            throw new FileNotFoundException($"WeatherKit private key file not found at: {keyLocation}");
        }

        var pemKey = File.ReadAllText(keyLocation);

        if (pemKey.Contains("-----BEGIN PRIVATE KEY-----"))
        {
            var base64Key = pemKey
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Trim();

            return Convert.FromBase64String(base64Key);
        }

        return File.ReadAllBytes(keyLocation);
    }
}
