using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using FastGooey.Models.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FastGooey.Services;

public interface IAppleSignInJwtService
{
    string GenerateClientSecret();
}

public class AppleSignInJwtService(
    IConfiguration configuration,
    ILogger<AppleSignInJwtService> logger) :
    IAppleSignInJwtService
{
    public string GenerateClientSecret()
    {
        var config = configuration.GetSection("Authentication:Apple").Get<SignInWithAppleConfigurationModel>();

        try
        {
            var privateKeyBytes = LoadPrivateKey(config.KeyLocation);

            var ecdsa = ECDsa.Create();
            ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

            var securityKey = new ECDsaSecurityKey(ecdsa) { KeyId = config.KeyId };
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.AddMinutes(5); // Apple allows up to 6 months

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = config.TeamId,
                Audience = "https://appleid.apple.com",
                Claims = new Dictionary<string, object>
                {
                    { "sub", config.ClientId } // Your Services ID
                },
                IssuedAt = issuedAt,
                Expires = expiresAt,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            securityToken.Header["typ"] = "JWT";
            securityToken.Header["kid"] = config.KeyId;
            securityToken.Header["alg"] = "ES256";

            return tokenHandler.WriteToken(securityToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating Apple Sign In client secret");
            throw;
        }
    }

    private byte[] LoadPrivateKey(string keyLocation)
    {
        if (!File.Exists(keyLocation))
        {
            throw new FileNotFoundException($"Apple Sign In private key file not found at: {keyLocation}");
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