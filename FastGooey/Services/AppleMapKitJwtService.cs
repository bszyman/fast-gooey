using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using FastGooey.Models.Configuration;
using FastGooey.Models.Response;
using Flurl.Http;
using Microsoft.IdentityModel.Tokens;

namespace FastGooey.Services;

public interface IAppleMapKitJwtService
{
    Task RefreshTokenAsync(CancellationToken cancellationToken = default);
}

public class AppleMapKitJwtService(
    ILogger<AppleMapKitJwtService> logger,
    IConfiguration configuration,
    IKeyValueService keyValueService) : IAppleMapKitJwtService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await GenerateAndSaveToken(cancellationToken);
            await FetchAndSaveMapKitServerToken();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<string> GetCurrentToken()
    {
        var token = await keyValueService.GetValueForKey(Constants.MapKitJwt);
        return token ?? throw new InvalidOperationException("MapKit JWT token has not been initialized yet.");
    }

    private async Task GenerateAndSaveToken(CancellationToken cancellationToken = default)
    {
        var config = configuration.GetSection("uCupertino:mapkit").Get<MapKitConfigurationModel>();

        if (config == null)
        {
            logger.LogError("MapKit configuration not found");
            throw new InvalidOperationException("MapKit configuration is missing");
        }

        // Validate configuration parameters
        if (string.IsNullOrEmpty(config.Origin) ||
            string.IsNullOrEmpty(config.KeyId) ||
            string.IsNullOrEmpty(config.TeamId) ||
            string.IsNullOrEmpty(config.KeyLocation))
        {
            logger.LogError("MapKit configuration is incomplete. Required: Origin, KeyId, TeamId, KeyLocation");
            throw new InvalidOperationException("MapKit configuration is incomplete");
        }

        try
        {
            var token = GenerateJwtToken(config);

            await keyValueService.SetValueForKey(Constants.MapKitJwt, token);
            await keyValueService.SetValueForKey(Constants.MapKitLastRefreshKey, DateTime.UtcNow.ToString("O"));

            logger.LogInformation("MapKit JWT token refreshed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating Apple MapKit JWT");
            throw;
        }
    }

    private string GenerateJwtToken(MapKitConfigurationModel config)
    {
        // Load and parse the private key
        var privateKeyBytes = LoadPrivateKey(config.KeyLocation);

        // Create the ECDsa key
        var ecdsa = ECDsa.Create();
        ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

        // Create security key and signing credentials
        var securityKey = new ECDsaSecurityKey(ecdsa) { KeyId = config.KeyId };
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

        // Set timestamps
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddHours(6);

        // Create JWT token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = config.TeamId,
            Audience = config.Origin,
            Claims = new Dictionary<string, object>
            {
                { "sub", config.TeamId },
                { "origin", config.Origin }
            },
            NotBefore = issuedAt,
            IssuedAt = issuedAt,
            Expires = expiresAt,
            SigningCredentials = signingCredentials
        };

        // Create and customize the token
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

        // Ensure header contains required fields
        securityToken.Header["typ"] = "JWT";
        securityToken.Header["kid"] = config.KeyId;

        return tokenHandler.WriteToken(securityToken);
    }

    private byte[] LoadPrivateKey(string keyLocation)
    {
        if (!File.Exists(keyLocation))
        {
            throw new FileNotFoundException($"MapKit private key file not found at: {keyLocation}");
        }

        var pemKey = File.ReadAllText(keyLocation);

        // Extract key from PEM format if needed
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

    private async Task FetchAndSaveMapKitServerToken()
    {
        var jwt = await GetCurrentToken();
        var response = await "https://maps-api.apple.com/v1/token"
            .WithHeader("Authorization", $"Bearer {jwt}")
            .GetJsonAsync<MapKitServerTokenResponse>();

        await keyValueService.SetValueForKey(
            key: Constants.MapKitServerKey,
            value: response.AccessToken
        );
    }
}