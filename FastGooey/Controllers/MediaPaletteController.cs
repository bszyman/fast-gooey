using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models.Media;
using FastGooey.Models.ViewModels.Media;
using FastGooey.Services;
using FastGooey.Services.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FastGooey.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/MediaPalette")]
public class MediaPaletteController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    IMediaSourceProviderRegistry providerRegistry,
    IMemoryCache memoryCache) :
    BaseStudioController(keyValueService, dbContext)
{
    private const int DefaultPageSize = 16;
    private static readonly TimeSpan ListCacheDuration = TimeSpan.FromMinutes(2);

    [HttpGet("ContentList")]
    public async Task<IActionResult> ContentList([FromRoute] Guid workspaceId, CancellationToken cancellationToken)
    {
        var workspace = await dbContext.Workspaces
            .Include(w => w.MediaSources)
            .FirstOrDefaultAsync(x => x.PublicId == workspaceId, cancellationToken);

        if (workspace is null)
        {
            return PartialView("~/Views/MediaPalette/ContentList.cshtml", new MediaPaletteViewModel
            {
                WorkspaceId = workspaceId
            });
        }

        var sources = workspace.MediaSources
            .Where(source => source.IsEnabled)
            .OrderBy(source => source.Name)
            .ToList();

        var viewModel = new MediaPaletteViewModel
        {
            WorkspaceId = workspace.PublicId,
            Sources = await BuildSourceSections(workspace.PublicId, sources, DefaultPageSize, cancellationToken)
        };

        return PartialView("~/Views/MediaPalette/ContentList.cshtml", viewModel);
    }

    [HttpGet("SourceSection/{sourceId:guid}")]
    public async Task<IActionResult> SourceSection(Guid sourceId, int? take, CancellationToken cancellationToken)
    {
        var source = await dbContext.MediaSources
            .Include(s => s.Workspace)
            .FirstOrDefaultAsync(s => s.PublicId == sourceId && s.Workspace.PublicId == WorkspaceId, cancellationToken);

        if (source is null || !source.IsEnabled)
        {
            return NotFound();
        }

        var requested = take.GetValueOrDefault(DefaultPageSize);
        var section = await BuildSourceSection(WorkspaceId, source, requested, cancellationToken);

        return PartialView("~/Views/MediaPalette/Partials/SourceSection.cshtml", section);
    }

    private async Task<IReadOnlyList<MediaPaletteSourceViewModel>> BuildSourceSections(
        Guid workspaceId,
        IReadOnlyList<MediaSource> sources,
        int take,
        CancellationToken cancellationToken)
    {
        var sections = new List<MediaPaletteSourceViewModel>();
        foreach (var source in sources)
        {
            sections.Add(await BuildSourceSection(workspaceId, source, take, cancellationToken));
        }

        return sections;
    }

    private async Task<MediaPaletteSourceViewModel> BuildSourceSection(
        Guid workspaceId,
        MediaSource source,
        int take,
        CancellationToken cancellationToken)
    {
        var viewModel = new MediaPaletteSourceViewModel
        {
            WorkspaceId = workspaceId,
            SourceId = source.PublicId,
            Name = source.Name,
            SourceType = source.SourceType.ToDisplayName(),
            DetailLine = BuildSourceDetailLine(source),
            RequestedCount = take
        };

        try
        {
            var items = await GetCachedItems(source, cancellationToken);
            var imageItems = items
                .Where(item => !item.IsFolder && IsImageItem(item))
                .OrderBy(item => item.Name)
                .ToList();

            var limited = imageItems
                .Take(take)
                .Select(item => new MediaItemViewModel
                {
                    Name = item.Name,
                    Path = item.Path,
                    IsFolder = item.IsFolder,
                    ContentType = item.ContentType,
                    Size = item.Size,
                    IsImage = true
                })
                .ToList();

            viewModel.Items = limited;
            viewModel.HasMore = imageItems.Count > take;
            viewModel.NextCount = Math.Min(take + DefaultPageSize, imageItems.Count);
        }
        catch (Exception)
        {
            viewModel.StatusLine = "Unable to reach this source.";
            viewModel.Items = [];
            viewModel.HasMore = false;
            viewModel.NextCount = take;
        }

        return viewModel;
    }

    private async Task<IReadOnlyList<MediaItem>> GetCachedItems(MediaSource source, CancellationToken cancellationToken)
    {
        var cacheKey = $"media-palette-list:{source.PublicId}:root";
        if (!memoryCache.TryGetValue(cacheKey, out IReadOnlyList<MediaItem>? items))
        {
            var provider = providerRegistry.GetProvider(source.SourceType);
            items = await provider.ListAsync(source, null, cancellationToken);
            memoryCache.Set(cacheKey, items, ListCacheDuration);
        }

        return items ?? [];
    }

    private static bool IsImageItem(MediaItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.ContentType))
        {
            return item.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        var contentType = GuessContentType(item.Name);
        return contentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private static string? GuessContentType(string fileName)
    {
        var provider = new FileExtensionContentTypeProvider();
        return provider.TryGetContentType(fileName, out var contentType) ? contentType : null;
    }

    private static string? BuildSourceDetailLine(MediaSource source)
    {
        return source.SourceType switch
        {
            MediaSourceType.S3 => string.IsNullOrWhiteSpace(source.S3BucketName) ? null : $"S3 | {source.S3BucketName}",
            MediaSourceType.AzureBlob => string.IsNullOrWhiteSpace(source.AzureContainerName) ? null : $"Azure | {source.AzureContainerName}",
            MediaSourceType.WebDav => string.IsNullOrWhiteSpace(source.WebDavBaseUrl) ? null : source.WebDavBaseUrl,
            _ => null
        };
    }
}
