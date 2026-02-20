using FastGooey.Attributes;
using FastGooey.Controllers;
using FastGooey.Database;
using FastGooey.Features.Media.LinkEditor.Models.ViewModels.LinkEditor;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FastGooey.Features.Media.LinkEditor.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/LinkEditor")]
public class LinkEditorController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    IMemoryCache memoryCache) :
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet("ContentList")]
    public async Task<IActionResult> ContentList([FromRoute] Guid workspaceId)
    {
        var cacheKey = $"link-editor-content-list:{workspaceId}";
        if (!memoryCache.TryGetValue(cacheKey, out List<LinkEditorContentNode>? nodes))
        {
            nodes = await dbContext.GooeyInterfaces
                .AsNoTracking()
                .Where(x => x.Workspace.PublicId.Equals(workspaceId))
                .Select(x => new LinkEditorContentNode
                {
                    Id = x.DocId,
                    Name = x.Name,
                    Platform = x.Platform,
                    ViewType = x.ViewType ?? string.Empty
                })
                .ToListAsync();

            memoryCache.Set(cacheKey, nodes, TimeSpan.FromSeconds(10));
        }

        var viewModel = new LinkEditorViewModel
        {
            WorkspaceId = workspaceId,
            AppleMobileNodes = nodes!
                .Where(x => x.Platform.Equals("AppleMobile"))
                .OrderBy(x => x.Name)
                .ToList(),
            MacNodes = nodes!
                .Where(x => x.Platform.Equals("Mac"))
                .OrderBy(x => x.Name)
                .ToList(),
            AppleTvNodes = nodes!
            .Where(x => x.Platform.Equals("AppleTv"))
            .OrderBy(x => x.Name)
            .ToList()
        };

        return PartialView("ContentList", viewModel);
    }
}
