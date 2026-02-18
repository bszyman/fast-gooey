using FastGooey.Database;
using FastGooey.Services.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FastGooey.Features.Media.Public.Controllers;

[AllowAnonymous]
[Route("public/media")]
public class PublicMediaController(
    ApplicationDbContext dbContext,
    IMediaSourceProviderRegistry providerRegistry,
    IMemoryCache memoryCache) : Controller
{
    private const long MaxPreviewCacheBytes = 10 * 1024 * 1024;
    private static readonly TimeSpan PreviewCacheDuration = TimeSpan.FromMinutes(10);

    [HttpGet("{workspaceId:guid}/preview/{sourceId:guid}")]
    public async Task<IActionResult> Preview(
        Guid workspaceId,
        Guid sourceId,
        string path,
        CancellationToken cancellationToken)
    {
        var source = await dbContext.MediaSources
            .FirstOrDefaultAsync(
                s => s.PublicId == sourceId &&
                     s.Workspace.PublicId == workspaceId &&
                     s.IsEnabled,
                cancellationToken);

        if (source is null)
        {
            return NotFound();
        }

        var provider = providerRegistry.GetProvider(source.SourceType);
        var cacheKey = $"media-preview-public:{workspaceId}:{source.PublicId}:{path}";

        if (memoryCache.TryGetValue(cacheKey, out CachedMediaFile? cached))
        {
            if (cached is not null &&
                cached.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return File(cached.Bytes, cached.ContentType, enableRangeProcessing: true);
            }

            memoryCache.Remove(cacheKey);
        }

        var streamResult = await provider.OpenReadAsync(source, path, cancellationToken);
        if (streamResult is null)
        {
            return NotFound();
        }

        var contentType = streamResult.ContentType ?? GuessContentType(path) ?? "application/octet-stream";
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status502BadGateway);
        }

        if (streamResult.ContentLength.HasValue && streamResult.ContentLength.Value <= MaxPreviewCacheBytes)
        {
            await using var stream = streamResult.Stream;
            await using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, cancellationToken);
            var bytes = buffer.ToArray();

            memoryCache.Set(cacheKey, new CachedMediaFile(bytes, contentType), PreviewCacheDuration);
            return File(bytes, contentType, enableRangeProcessing: true);
        }

        return File(streamResult.Stream, contentType, enableRangeProcessing: true);
    }

    private static string? GuessContentType(string fileName)
    {
        var provider = new FileExtensionContentTypeProvider();
        return provider.TryGetContentType(fileName, out var contentType) ? contentType : null;
    }

    private sealed record CachedMediaFile(byte[] Bytes, string ContentType);
}
