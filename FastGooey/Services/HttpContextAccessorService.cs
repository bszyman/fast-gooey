namespace FastGooey.Services;

public interface IHttpContextAccessorService
{
    HttpContext? HttpContext { get; }
    string GetRemoteIpAddress();
}

public class HttpContextAccessorService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextAccessorService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public HttpContext? HttpContext => _httpContextAccessor.HttpContext;
    
    string GetRemoteIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return string.Empty;

        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (string.IsNullOrEmpty(ip))
        {
            ip = context.Connection.RemoteIpAddress?.ToString();
        }
        else
        {
            ip = ip.Split(',').First().Trim();
        }
        
        return ip ?? string.Empty;
    }
}