namespace FastGooey.Services;

public interface IHttpContextAccessorService
{
    HttpContext? HttpContext { get; }
    public string GetRemoteIpAddress();
}

public class HttpContextAccessorService : IHttpContextAccessorService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextAccessorService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public HttpContext? HttpContext => _httpContextAccessor.HttpContext;

    public string GetRemoteIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
            return string.Empty;

        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (string.IsNullOrEmpty(ip))
            ip = context.Connection.RemoteIpAddress?.ToString();
        else
            ip = ip.Split(',').First().Trim();

        return ip ?? string.Empty;
    }
}