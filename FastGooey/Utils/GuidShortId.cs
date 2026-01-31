using System;

namespace FastGooey.Utils;

public static class GuidShortId
{
    public static string ToBase64Url(this Guid guid)
    {
        var bytes = guid.ToByteArray();
        var base64 = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return base64;
    }

    public static string ToBase64Url(this Guid? guid)
    {
        return guid.HasValue ? guid.Value.ToBase64Url() : string.Empty;
    }

    public static bool TryParse(string value, out Guid guid)
    {
        if (Guid.TryParse(value, out guid))
        {
            return true;
        }

        var normalized = value.Replace('-', '+').Replace('_', '/');
        switch (normalized.Length % 4)
        {
            case 2:
                normalized += "==";
                break;
            case 3:
                normalized += "=";
                break;
            case 0:
                break;
            default:
                guid = default;
                return false;
        }

        try
        {
            guid = new Guid(Convert.FromBase64String(normalized));
            return true;
        }
        catch
        {
            guid = default;
            return false;
        }
    }
}
