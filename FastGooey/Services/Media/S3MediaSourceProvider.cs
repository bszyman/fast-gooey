using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using FastGooey.Models.Media;

namespace FastGooey.Services.Media;

public class S3MediaSourceProvider(IMediaCredentialProtector credentialProtector) : IMediaSourceProvider
{
    public MediaSourceType SourceType => MediaSourceType.S3;

    public async Task<IReadOnlyList<MediaItem>> ListAsync(MediaSource source, string? path, CancellationToken cancellationToken)
    {
        using var client = CreateClient(source);
        var prefix = NormalizePrefix(path);

        var request = new ListObjectsV2Request
        {
            BucketName = EnsureValue(source.S3BucketName, "S3 bucket name is required."),
            Prefix = prefix,
            Delimiter = "/"
        };

        var response = await client.ListObjectsV2Async(request, cancellationToken);
        var items = new List<MediaItem>();

        if (response.CommonPrefixes is not null)
        {
            foreach (var folderPrefix in response.CommonPrefixes)
            {
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
        }

        foreach (var obj in response.S3Objects)
        {
            if (obj.Key.EndsWith("/", StringComparison.Ordinal) && obj.Size == 0)
            {
                continue;
            }

            var name = GetChildName(obj.Key, prefix);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            items.Add(new MediaItem
            {
                Name = name,
                Path = obj.Key,
                IsFolder = false,
                Size = obj.Size
            });
        }

        return items;
    }

    public async Task<MediaStreamResult?> OpenReadAsync(MediaSource source, string path, CancellationToken cancellationToken)
    {
        using var client = CreateClient(source);
        var request = new GetObjectRequest
        {
            BucketName = EnsureValue(source.S3BucketName, "S3 bucket name is required."),
            Key = path
        };

        var response = await client.GetObjectAsync(request, cancellationToken);
        return new MediaStreamResult(response.ResponseStream, response.Headers.ContentType, response.ContentLength);
    }

    private AmazonS3Client CreateClient(MediaSource source)
    {
        var accessKey = credentialProtector.Unprotect(source.S3AccessKeyId);
        var secretKey = credentialProtector.Unprotect(source.S3SecretAccessKey);
        AWSCredentials credentials = string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey)
            ? new AnonymousAWSCredentials()
            : new BasicAWSCredentials(accessKey, secretKey);

        var config = new AmazonS3Config();
        if (!string.IsNullOrWhiteSpace(source.S3Region))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(source.S3Region);
        }

        if (!string.IsNullOrWhiteSpace(source.S3ServiceUrl))
        {
            config.ServiceURL = source.S3ServiceUrl;
            config.ForcePathStyle = true;
        }

        return new AmazonS3Client(credentials, config);
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
