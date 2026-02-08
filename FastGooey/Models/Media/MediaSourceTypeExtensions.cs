namespace FastGooey.Models.Media;

public static class MediaSourceTypeExtensions
{
    public static string ToDisplayName(this MediaSourceType sourceType)
    {
        return sourceType switch
        {
            MediaSourceType.S3 => "Amazon S3",
            MediaSourceType.AzureBlob => "Azure Blob Storage",
            MediaSourceType.WebDav => "WebDAV",
            _ => sourceType.ToString()
        };
    }
}
