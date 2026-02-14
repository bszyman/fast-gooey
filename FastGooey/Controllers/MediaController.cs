using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models.Media;
using FastGooey.Models.ViewModels;
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
[Route("Workspaces/{workspaceId:guid}/[controller]")]
public class MediaController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    IMediaSourceProviderRegistry providerRegistry,
    IMemoryCache memoryCache) :
    BaseStudioController(keyValueService, dbContext)
{
    private const long MaxPreviewCacheBytes = 10 * 1024 * 1024;
    private static readonly TimeSpan ListCacheDuration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PreviewCacheDuration = TimeSpan.FromMinutes(10);

    [HttpGet]
    public IActionResult Index()
    {
        var workspace = dbContext.Workspaces
            .First(x => x.PublicId == WorkspaceId);

        var viewModel = new MediaIndexViewModel
        {
            Workspace = workspace,
            NavBarViewModel = new MetalNavBarViewModel
            {
                WorkspaceName = workspace.Name,
                WorkspaceId = workspace.PublicId,
                ActiveTab = "Media"
            }
        };

        return View(viewModel);
    }

    [HttpGet("sources")]
    public async Task<IActionResult> SourceList(CancellationToken cancellationToken)
    {
        var workspace = await dbContext.Workspaces
            .Include(w => w.MediaSources)
            .FirstAsync(x => x.PublicId == WorkspaceId, cancellationToken);

        var sources = workspace.MediaSources
            .Where(source => source.IsEnabled)
            .OrderBy(source => source.Name)
            .ToList();

        var listItems = new List<MediaSourceListItemViewModel>();
        foreach (var source in sources)
        {
            List<MediaFolderViewModel> rootFolders;
            string? statusLine = null;

            try
            {
                var provider = providerRegistry.GetProvider(source.SourceType);
                var items = await provider.ListAsync(source, null, cancellationToken);

                rootFolders = items
                    .Where(item => item.IsFolder)
                    .OrderBy(item => item.Name)
                    .Select(item => new MediaFolderViewModel
                    {
                        Name = item.Name,
                        Path = item.Path
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                rootFolders = [];
                statusLine = "Unable to reach this source.";
            }

            listItems.Add(new MediaSourceListItemViewModel
            {
                SourceId = source.PublicId,
                Name = source.Name,
                SourceType = source.SourceType.ToDisplayName(),
                DetailLine = BuildSourceDetailLine(source),
                StatusLine = statusLine,
                RootFolders = rootFolders
            });
        }

        var viewModel = new MediaSourceListViewModel
        {
            WorkspaceId = workspace.PublicId,
            Sources = listItems
        };

        return PartialView("~/Views/Media/Partials/SourceList.cshtml", viewModel);
    }

    [HttpGet("browse/{sourceId:guid}")]
    public async Task<IActionResult> Browse(Guid sourceId, string? path, CancellationToken cancellationToken)
    {
        var source = await dbContext.MediaSources
            .FirstOrDefaultAsync(s => s.PublicId == sourceId && s.Workspace.PublicId == WorkspaceId, cancellationToken);

        if (source is null) return NotFound();
        if (!source.IsEnabled) return NotFound();

        var normalizedPath = NormalizePath(path);
        IReadOnlyList<MediaItem> items;
        string? errorMessage = null;

        if (!source.IsEnabled)
        {
            items = [];
            errorMessage = "This source is disabled in workspace settings.";
        }
        else
        {
            try
            {
                items = await GetCachedItems(source, normalizedPath, cancellationToken);
            }
            catch (Exception)
            {
                items = [];
                errorMessage = "Unable to load this folder. Check the source settings and credentials.";
            }
        }
        
        var viewModel = new MediaWorkspaceViewModel
        {
            WorkspaceId = WorkspaceId,
            Source = new MediaSourceListItemViewModel
            {
                SourceId = source.PublicId,
                Name = source.Name,
                SourceType = source.SourceType.ToDisplayName(),
                DetailLine = BuildSourceDetailLine(source)
            },
            CurrentPath = normalizedPath,
            ErrorMessage = errorMessage,
            Breadcrumbs = BuildBreadcrumbs(normalizedPath),
            Items = items
                .OrderByDescending(item => item.IsFolder)
                .ThenBy(item => item.Name)
                .Select(item => new MediaItemViewModel
                {
                    Name = item.Name,
                    Path = item.Path,
                    IsFolder = item.IsFolder,
                    ContentType = item.ContentType,
                    Size = item.Size,
                    IsImage = IsImageItem(item)
                })
                .ToList()
        };

        return PartialView("~/Views/Media/Workspace.cshtml", viewModel);
    }

    [HttpGet("preview/{sourceId:guid}")]
    public async Task<IActionResult> Preview(Guid sourceId, string path, CancellationToken cancellationToken)
    {
        var source = await dbContext.MediaSources
            .FirstOrDefaultAsync(s => s.PublicId == sourceId && s.Workspace.PublicId == WorkspaceId, cancellationToken);

        if (source is null)
        {
            return NotFound();
        }

        var provider = providerRegistry.GetProvider(source.SourceType);
        var cacheKey = $"media-preview:{WorkspaceId}:{source.PublicId}:{path}";
        
        if (memoryCache.TryGetValue(cacheKey, out CachedMediaFile? cached))
        {
            if (cached is not null &&
                cached.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return File(cached.Bytes, cached.ContentType, enableRangeProcessing: true);
            }

            memoryCache.Remove(cacheKey); // stale/bad cache entry (e.g. text/html)
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

    [HttpGet("preview-panel/{sourceId:guid}")]
    public IActionResult PreviewPanel(Guid sourceId, string path, string name, string? contentType)
    {
        var isValidSource = dbContext.MediaSources
            .Any(source => source.PublicId == sourceId && source.Workspace.PublicId == WorkspaceId);
        if (!isValidSource)
        {
            return NotFound();
        }

        var viewModel = new MediaPreviewViewModel
        {
            WorkspaceId = WorkspaceId,
            SourceId = sourceId,
            Path = path,
            Name = name,
            ContentType = contentType
        };

        return PartialView("~/Views/Media/Partials/MediaPreviewPanel.cshtml", viewModel);
    }

    private static string? NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return path.Trim('/');
    }

    private static IReadOnlyList<MediaBreadcrumbViewModel> BuildBreadcrumbs(string? path)
    {
        var breadcrumbs = new List<MediaBreadcrumbViewModel>
        {
            new() { Label = "Root", Path = null }
        };

        if (string.IsNullOrWhiteSpace(path))
        {
            return breadcrumbs;
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current = string.Empty;
        foreach (var segment in segments)
        {
            current = string.IsNullOrEmpty(current) ? segment : $"{current}/{segment}";
            breadcrumbs.Add(new MediaBreadcrumbViewModel
            {
                Label = segment,
                Path = current
            });
        }

        return breadcrumbs;
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

    private async Task<IReadOnlyList<MediaItem>> GetCachedItems(MediaSource source, string? path, CancellationToken cancellationToken)
    {
        var normalizedPath = NormalizePath(path);
        var cacheKey = $"media-source-list:{source.PublicId}:{normalizedPath ?? "root"}";
        if (!memoryCache.TryGetValue(cacheKey, out IReadOnlyList<MediaItem>? items))
        {
            var provider = providerRegistry.GetProvider(source.SourceType);
            items = await provider.ListAsync(source, normalizedPath, cancellationToken);
            memoryCache.Set(cacheKey, items, ListCacheDuration);
        }

        return items ?? [];
    }

    private sealed record CachedMediaFile(byte[] Bytes, string ContentType);
}
