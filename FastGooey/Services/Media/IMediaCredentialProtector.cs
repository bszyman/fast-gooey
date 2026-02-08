namespace FastGooey.Services.Media;

public interface IMediaCredentialProtector
{
    string? Protect(string? value);
    string? Unprotect(string? value);
}
