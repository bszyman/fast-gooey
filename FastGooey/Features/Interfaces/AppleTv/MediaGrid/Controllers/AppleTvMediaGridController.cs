using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Features.Interfaces.AppleTv.MediaGrid.Models;
using FastGooey.Features.Interfaces.Shared.Controllers;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.AppleTv.MediaGrid.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/tvOS/MediaGrid")]
public class AppleTvMediaGridController(
    ILogger<AppleTvMediaGridController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private async Task<MediaWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<MediaWorkspaceViewModel, AppleTvMediaGridJsonDataModel>(interfaceId);
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        return View(new MediaIndexViewModel
        {
            Workspace = await WorkspaceViewModelForInterfaceId(interfaceGuid)
        });
    }

    [HttpGet("workspace/{interfaceId}")]
    public async Task<IActionResult> Workspace(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        return PartialView("Workspace", await WorkspaceViewModelForInterfaceId(interfaceGuid));
    }

    [HttpPost("workspace/{interfaceId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveWorkspace(string interfaceId, [FromForm] MediaWorkspaceFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));
        var data = contentNode.Config.Deserialize<AppleTvMediaGridJsonDataModel>() ?? new AppleTvMediaGridJsonDataModel();

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#workspaceEditor");
            return PartialView("Workspace", new MediaWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            });
        }

        data.Title = formModel.Title.Trim();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        return PartialView("Workspace", new MediaWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = data
        });
    }

    [HttpGet("workspace/{interfaceId}/item/{itemId?}")]
    public async Task<IActionResult> MediaGridItemPanel(string interfaceId, Guid? itemId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));
        var data = contentNode.Config.Deserialize<AppleTvMediaGridJsonDataModel>() ?? new AppleTvMediaGridJsonDataModel();

        var item = itemId.HasValue
            ? data.MediaItems.FirstOrDefault(x => x.Guid.Equals(itemId.Value.ToString(), StringComparison.OrdinalIgnoreCase)) ?? new AppleTvMediaGridItemJsonDataModel()
            : new AppleTvMediaGridItemJsonDataModel();

        return PartialView("Partials/MediaGridItemPanel", new MediaGridItemPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            ItemId = item.Guid,
            Title = item.Title,
            LinkTo = item.LinkTo,
            PreviewMedia = item.PreviewMedia
        });
    }

    [HttpPost("workspace/{interfaceId}/item/{itemId?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMediaGridItemPanel(
        string interfaceId,
        Guid? itemId,
        [FromForm] MediaGridItemPanelFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));
        var data = contentNode.Config.Deserialize<AppleTvMediaGridJsonDataModel>() ?? new AppleTvMediaGridJsonDataModel();

        var item = itemId.HasValue
            ? data.MediaItems.FirstOrDefault(x => x.Guid.Equals(itemId.Value.ToString(), StringComparison.OrdinalIgnoreCase))
            : null;

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return PartialView("Partials/MediaGridItemPanel", new MediaGridItemPanelViewModel
            {
                WorkspaceId = contentNode.Workspace.PublicId,
                InterfaceId = contentNode.DocId,
                ItemId = item?.Guid ?? itemId?.ToString() ?? string.Empty,
                Title = formModel.Title,
                LinkTo = formModel.LinkTo,
                PreviewMedia = formModel.PreviewMedia
            });
        }

        if (item is null)
        {
            item = new AppleTvMediaGridItemJsonDataModel
            {
                Guid = Guid.NewGuid().ToString()
            };
            data.MediaItems.Add(item);
        }

        item.Title = formModel.Title.Trim();
        item.LinkTo = formModel.LinkTo.Trim();
        item.PreviewMedia = formModel.PreviewMedia.Trim();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Partials/MediaGridItemPanel", new MediaGridItemPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            ItemId = item.Guid,
            Title = item.Title,
            LinkTo = item.LinkTo,
            PreviewMedia = item.PreviewMedia
        });
    }

    [HttpPost("create-interface")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInterface()
    {
        if (await InterfaceLimitReachedAsync())
        {
            return Forbid();
        }

        var workspace = GetWorkspace();
        if (workspace is null)
        {
            return NotFound();
        }

        var data = new AppleTvMediaGridJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "MediaGrid",
            Name = "New Media Grid Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index", new MediaIndexViewModel
        {
            Workspace = new MediaWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        });
    }
}
