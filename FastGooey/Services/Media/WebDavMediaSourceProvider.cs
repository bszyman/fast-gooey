using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using FastGooey.Models.Media;

namespace FastGooey.Services.Media;

public class WebDavMediaSourceProvider(IHttpClientFactory httpClientFactory, IMediaCredentialProtector credentialProtector)
    : IMediaSourceProvider
{
    private static readonly XNamespace DavNamespace = "DAV:";

    public MediaSourceType SourceType => MediaSourceType.WebDav;

    public async Task<IReadOnlyList<MediaItem>> ListAsync(MediaSource source, string? path, CancellationToken cancellationToken)
    {
        var requestUrl = BuildCollectionUri(source, path);
        var request = new HttpRequestMessage(new HttpMethod("PROPFIND"), requestUrl)
        {
            Content = new StringContent(PropFindBody, Encoding.UTF8, "application/xml")
        };
        request.Headers.Add("Depth", "1");
        ApplyAuthentication(request, source);

        var client = httpClientFactory.CreateClient(nameof(WebDavMediaSourceProvider));
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = XDocument.Load(responseStream);

        var baseUri = new Uri(EnsureValue(source.WebDavBaseUrl, "WebDAV base URL is required."));
        var items = new List<MediaItem>();
        var normalizedPath = NormalizePath(path);

        foreach (var responseNode in document.Descendants(DavNamespace + "response"))
        {
            var href = responseNode.Element(DavNamespace + "href")?.Value;
            if (string.IsNullOrWhiteSpace(href))
            {
                continue;
            }

            var itemUri = new Uri(baseUri, href);
            var relative = Uri.UnescapeDataString(baseUri.MakeRelativeUri(itemUri).ToString());
            var trimmed = relative.TrimEnd('/');

            if (string.Equals(trimmed, normalizedPath ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var isCollection = responseNode
                .Descendants(DavNamespace + "resourcetype")
                .Elements(DavNamespace + "collection")
                .Any();

            var name = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            items.Add(new MediaItem
            {
                Name = name,
                Path = trimmed,
                IsFolder = isCollection
            });
        }

        return items;
    }

    public async Task<MediaStreamResult?> OpenReadAsync(MediaSource source, string path, CancellationToken cancellationToken)
    {
        var requestUrl = BuildItemUri(source, path);
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        ApplyAuthentication(request, source);

        var client = httpClientFactory.CreateClient(nameof(WebDavMediaSourceProvider));
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType;
        var contentLength = response.Content.Headers.ContentLength;

        return new MediaStreamResult(stream, contentType, contentLength);
    }

    private void ApplyAuthentication(HttpRequestMessage request, MediaSource source)
    {
        if (!source.WebDavUseBasicAuth)
        {
            return;
        }

        var username = credentialProtector.Unprotect(source.WebDavUsername) ?? string.Empty;
        var password = credentialProtector.Unprotect(source.WebDavPassword) ?? string.Empty;
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
    }

    private static Uri BuildCollectionUri(MediaSource source, string? path)
    {
        var baseUri = new Uri(EnsureValue(source.WebDavBaseUrl, "WebDAV base URL is required."));
        if (!baseUri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal))
        {
            baseUri = new Uri($"{baseUri.AbsoluteUri}/");
        }

        var normalized = NormalizePath(path);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return baseUri;
        }

        return new Uri(baseUri, $"{normalized}/");
    }

    private static Uri BuildItemUri(MediaSource source, string path)
    {
        var baseUri = new Uri(EnsureValue(source.WebDavBaseUrl, "WebDAV base URL is required."));
        if (!baseUri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal))
        {
            baseUri = new Uri($"{baseUri.AbsoluteUri}/");
        }

        var normalized = NormalizePath(path) ?? string.Empty;
        return new Uri(baseUri, normalized);
    }

    private static string? NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return path.Trim('/');
    }

    private static string EnsureValue(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }

        return value;
    }

    private const string PropFindBody = """
                                         <?xml version="1.0" encoding="utf-8"?>
                                         <d:propfind xmlns:d="DAV:">
                                           <d:prop>
                                             <d:resourcetype />
                                             <d:getcontentlength />
                                             <d:getcontenttype />
                                           </d:prop>
                                         </d:propfind>
                                         """;
}
