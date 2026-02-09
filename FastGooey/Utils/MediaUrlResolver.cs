using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Utils;

public static class MediaUrlResolver
{
    private const string Scheme = "fastgooey:media:";

    public static string Resolve(IUrlHelper urlHelper, Guid workspaceId, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
        }

        if (!value.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        var remainder = value[Scheme.Length..];
        var separatorIndex = remainder.IndexOf(':');
        if (separatorIndex <= 0 || separatorIndex >= remainder.Length - 1)
        {
            return value;
        }

        var sourceValue = remainder[..separatorIndex];
        if (!Guid.TryParse(sourceValue, out var sourceId))
        {
            return value;
        }

        var encodedPath = remainder[(separatorIndex + 1)..];
        var path = Uri.UnescapeDataString(encodedPath);
        var url = urlHelper.Action("Preview", "Media", new { workspaceId, sourceId, path });
        return url ?? value;
    }
    
    public static string Resolve(IUrlHelper urlHelper, string workspaceId, string? value)
    {
        return Resolve(urlHelper, Guid.Parse(workspaceId), value);
    }
}
