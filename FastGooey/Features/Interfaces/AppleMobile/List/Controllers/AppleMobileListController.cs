using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Features.Interfaces.AppleMobile.List.Models;
using FastGooey.Features.Interfaces.Shared.Controllers;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.AppleMobile.List.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/AppleMobile/List")]
public class AppleMobileListController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private async Task<AppleMobileInterfaceListWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<AppleMobileInterfaceListWorkspaceViewModel, AppleMobileListJsonDataModel>(interfaceId);
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        var viewModel = new AppleMobileInterfaceListViewModel
        {
            WorkspaceViewModel = workspaceViewModel
        };

        return View(viewModel);
    }

    [HttpGet("workspace/{interfaceId}")]
    public async Task<IActionResult> ListWorkspace(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        return PartialView("Workspace", viewModel);
    }

    [HttpPost("create-interface")]
    public async Task<IActionResult> CreateInterface()
    {
        if (await InterfaceLimitReachedAsync())
        {
            return Forbid();
        }

        var workspace = GetWorkspace();
        var data = new AppleMobileListJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "AppleMobile",
            ViewType = "List",
            Name = "New List Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var viewModel = new AppleMobileInterfaceListViewModel
        {
            WorkspaceViewModel = new AppleMobileInterfaceListWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index", viewModel);
    }

    [HttpGet("{interfaceId}/item-editor-panel/item/{itemId:guid?}")]
    public IActionResult ListItemEditorPanel(string interfaceId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var editorItem = new AppleMobileListItemJsonDataModel();

        if (itemId.HasValue)
        {
            var contentNode = dbContext.GooeyInterfaces
                .First(x => x.DocId.Equals(interfaceGuid));
            var data = contentNode.Config.Deserialize<AppleMobileListJsonDataModel>();

            var item = data.Items
                .FirstOrDefault(x => x.Identifier.Equals(itemId.Value));

            if (item is not null)
            {
                editorItem = item;
            }
        }

        var viewModel = new AppleMobileInterfaceListEditorViewModel
        {
            WorkspaceId = WorkspaceId,
            InterfaceId = interfaceGuid,
            Item = editorItem
        };

        return PartialView("Partials/ListItemEditorPanel", viewModel);
    }

    [HttpPost("{interfaceId}/item-editor-panel/item/{itemId:guid?}")]
    public async Task<IActionResult> ListItemEditorPanelWithItem(string interfaceId, Guid? itemId, [FromForm] AppleMobileListEditorPanelFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<AppleMobileListJsonDataModel>();

        AppleMobileListItemJsonDataModel? item = null;

        if (itemId.HasValue)
        {
            item = data.Items.FirstOrDefault(x => x.Identifier.Equals(itemId.Value));
        }

        if (!ModelState.IsValid)
        {
            item ??= new AppleMobileListItemJsonDataModel
            {
                Identifier = itemId ?? Guid.Empty
            };

            item.Title = formModel.Title;
            item.Subtitle = formModel.Subtitle;
            item.Url = formModel.Url;

            Response.Headers.Append("HX-Retarget", "#editorPanel");
        }
        else
        {
            if (item is null)
            {
                item = new AppleMobileListItemJsonDataModel
                {
                    Identifier = Guid.NewGuid()
                };

                data.Items = data.Items.Append(item).ToList();
            }

            item.Title = formModel.Title;
            item.Subtitle = formModel.Subtitle;
            item.Url = formModel.Url;

            contentNode.Config = JsonSerializer.SerializeToDocument(data);
            await dbContext.SaveChangesAsync();

            Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");
        }

        var viewModel = new AppleMobileInterfaceListEditorViewModel
        {
            WorkspaceId = WorkspaceId,
            InterfaceId = interfaceGuid,
            Item = item
        };

        return PartialView("Partials/ListItemEditorPanel", viewModel);
    }

    [HttpDelete("{interfaceId}/item/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(string interfaceId, Guid itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<AppleMobileListJsonDataModel>();
        var item = data.Items.FirstOrDefault(x => x.Identifier.Equals(itemId));

        if (item is null)
        {
            return NotFound();
        }

        data.Items = data.Items
            .Where(x => !x.Identifier.Equals(itemId))
            .ToList();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        var viewModel = new AppleMobileInterfaceListWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = data
        };

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Workspace", viewModel);
    }
}
