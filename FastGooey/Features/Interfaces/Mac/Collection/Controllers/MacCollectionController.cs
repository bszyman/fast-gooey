using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Features.Interfaces.Mac.Collection.Models;
using FastGooey.Features.Interfaces.Shared.Controllers;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.Mac.Collection.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/interfaces/mac/collection")]
public class MacCollectionController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private async Task<MacInterfaceCollectionWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<MacInterfaceCollectionWorkspaceViewModel, MacCollectionViewJsonDataModel>(interfaceId);
    }

    [HttpPost("create-interface")]
    public async Task<IActionResult> CreateInterface()
    {
        if (await InterfaceLimitReachedAsync())
        {
            return Forbid();
        }

        var workspace = GetWorkspace();
        var data = new MacCollectionViewJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "Mac",
            ViewType = "Collection",
            Name = "New Collection Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var viewModel = new MacInterfaceCollectionViewModel
        {
            Workspace = new MacInterfaceCollectionWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index", viewModel);
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        var viewModel = new MacInterfaceCollectionViewModel
        {
            Workspace = workspaceViewModel
        };

        return View(viewModel);
    }

    [HttpGet("workspace/{interfaceId}")]
    public async Task<IActionResult> Workspace(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        return PartialView("Workspace", viewModel);
    }

    [HttpGet("{interfaceId}/item-editor-panel/item/{itemId:guid?}")]
    public IActionResult CollectionViewItemEditorPanel(string interfaceId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var editorItem = new MacCollectionViewItemJsonDataModel();

        if (itemId.HasValue)
        {
            var contentNode = dbContext.GooeyInterfaces
                .First(x => x.DocId.Equals(interfaceGuid));
            var data = contentNode.Config.Deserialize<MacCollectionViewJsonDataModel>();

            var item = data.Items
                .FirstOrDefault(x => x.Identifier.Equals(itemId.Value));

            if (item is not null)
            {
                editorItem = item;
            }
        }

        var viewModel = new MacInterfaceCollectionEditorPanelViewModel
        {
            WorkspaceId = WorkspaceId,
            InterfaceId = interfaceGuid,
            Item = editorItem
        };

        return PartialView("Partials/CollectionViewItemEditorPanel", viewModel);
    }

    [HttpPost("{interfaceId}/item-editor-panel/item/{itemId:guid?}")]
    public async Task<IActionResult> CollectionViewItemEditorPanelWithItem(string interfaceId, Guid? itemId,
        [FromForm] MacCollectionEditorPanelFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacCollectionViewJsonDataModel>();

        MacCollectionViewItemJsonDataModel? item = null;

        if (itemId.HasValue)
        {
            item = data.Items.FirstOrDefault(x => x.Identifier.Equals(itemId.Value));
        }

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");

            var invalidViewModel = new MacInterfaceCollectionEditorPanelViewModel
            {
                WorkspaceId = WorkspaceId,
                InterfaceId = interfaceGuid,
                Item = new MacCollectionViewItemJsonDataModel
                {
                    Identifier = item?.Identifier ?? Guid.Empty,
                    Title = formModel.Title,
                    ImageUrl = formModel.ImageUrl ?? string.Empty,
                    Url = formModel.Url ?? string.Empty
                }
            };

            return PartialView("Partials/CollectionViewItemEditorPanel", invalidViewModel);
        }

        if (item is null)
        {
            item = new MacCollectionViewItemJsonDataModel
            {
                Identifier = Guid.NewGuid()
            };

            data.Items.Add(item);
        }

        item.Title = formModel.Title;
        item.ImageUrl = formModel.ImageUrl ?? string.Empty;
        item.Url = formModel.Url ?? string.Empty;

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        var viewModel = new MacInterfaceCollectionEditorPanelViewModel
        {
            WorkspaceId = WorkspaceId,
            InterfaceId = interfaceGuid,
            Item = item ?? new MacCollectionViewItemJsonDataModel()
        };

        return PartialView("Partials/CollectionViewItemEditorPanel", viewModel);
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

        var data = contentNode.Config.Deserialize<MacCollectionViewJsonDataModel>();
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

        var viewModel = new MacInterfaceCollectionWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = data
        };

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Workspace", viewModel);
    }
}
