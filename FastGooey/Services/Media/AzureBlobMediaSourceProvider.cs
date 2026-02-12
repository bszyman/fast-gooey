using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FastGooey.Models.Media;

namespace FastGooey.Services.Media;

public class AzureBlobMediaSourceProvider(IMediaCredentialProtector credentialProtector) : IMediaSourceProvider
{
    public MediaSourceType SourceType => MediaSourceType.AzureBlob;

    public async Task<IReadOnlyList<MediaItem>> ListAsync(MediaSource source, string? path, CancellationToken cancellationToken)
    {
        var container = CreateContainerClient(source);
        var prefix = NormalizePrefix(path);

        var items = new List<MediaItem>();
        await foreach (var blob in container.GetBlobsByHierarchyAsync(
                           prefix: prefix,
                           delimiter: "/",
                           cancellationToken: cancellationToken))
        {
            if (blob.IsPrefix)
            {
                var folderPrefix = blob.Prefix ?? string.Empty;
                var name = GetChildName(folderPrefix, prefix);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                items.Add(new MediaItem
                {
                    Name = name,
                    Path = folderPrefix.TrimEnd('/'),
                    IsFolder = true
                });
            }
            else
            {
                var item = blob.Blob;
                if (item is null)
                {
                    continue;
                }

                var name = GetChildName(item.Name, prefix);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                items.Add(new MediaItem
                {
                    Name = name,
                    Path = item.Name,
                    IsFolder = false,
                    Size = item.Properties.ContentLength,
                    ContentType = item.Properties.ContentType
                });
            }
        }

        return items;
    }

    public async Task<MediaStreamResult?> OpenReadAsync(MediaSource source, string path, CancellationToken cancellationToken)
    {
        var container = CreateContainerClient(source);
        var blobClient = container.GetBlobClient(path);
        var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

        return new MediaStreamResult(
            response.Value.Content,
            response.Value.Details.ContentType,
            response.Value.Details.ContentLength);
    }

    private BlobContainerClient CreateContainerClient(MediaSource source)
    {
        var connectionString = credentialProtector.Unprotect(source.AzureConnectionString);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Azure connection string is required.");
        }

        var containerName = EnsureValue(source.AzureContainerName, "Azure container name is required.");
        return new BlobContainerClient(connectionString, containerName);
    }

    private static string NormalizePrefix(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var trimmed = path.Trim('/');
        return string.IsNullOrEmpty(trimmed) ? string.Empty : $"{trimmed}/";
    }

    private static string GetChildName(string key, string prefix)
    {
        if (!string.IsNullOrEmpty(prefix) && key.StartsWith(prefix, StringComparison.Ordinal))
        {
            key = key[prefix.Length..];
        }

        return key.Trim('/');
    }

    private static string EnsureValue(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }

        return value;
    }
}
