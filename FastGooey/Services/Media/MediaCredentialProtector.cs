using Microsoft.AspNetCore.DataProtection;

namespace FastGooey.Services.Media;

public class MediaCredentialProtector(IDataProtectionProvider dataProtectionProvider) : IMediaCredentialProtector
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("FastGooey.MediaCredentials");

    public string? Protect(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _protector.Protect(value);
    }

    public string? Unprotect(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _protector.Unprotect(value);
    }
}
