using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Features.Interfaces.AppleMobile.Shared.Models.FormModels;
using FastGooey.Features.Interfaces.AppleMobile.Shared.Models.ViewModels.AppleMobileInterface;
using FastGooey.Features.Interfaces.Shared.Controllers;
using FastGooey.Models;
using FastGooey.Models.JsonDataModels;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.AppleMobile.Collection.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/AppleMobile/Collection")]
public class AppleMobileCollectionController(
    ILogger<AppleMobileCollectionController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private async Task<AppleMobileInterfaceCollectionWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<AppleMobileInterfaceCollectionWorkspaceViewModel, AppleMobileCollectionViewJsonDataModel>(interfaceId);
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        var viewModel = new AppleMobileInterfaceCollectionViewModel
        {
            WorkspaceViewModel = workspaceViewModel
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

    [HttpPost("create-interface")]
    public async Task<IActionResult> CreateInterface()
    {
        if (await InterfaceLimitReachedAsync())
        {
            return Forbid();
        }

        var workspace = GetWorkspace();
        var data = new AppleMobileCollectionViewJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "AppleMobile",
            ViewType = "Collection",
            Name = "New Collection Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var viewModel = new AppleMobileInterfaceCollectionViewModel
        {
            WorkspaceViewModel = new AppleMobileInterfaceCollectionWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index", viewModel);
    }

    [HttpGet("{interfaceId}/item-editor-panel/item/{itemId:guid?}")]
    public IActionResult CollectionViewItemEditorPanel(string interfaceId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var editorItem = new AppleMobileCollectionViewItemJsonDataModel();

        if (itemId.HasValue)
        {
            var contentNode = dbContext.GooeyInterfaces
                .First(x => x.DocId.Equals(interfaceGuid));
            var data = contentNode.Config.Deserialize<AppleMobileCollectionViewJsonDataModel>();

            var item = data.Items
                .FirstOrDefault(x => x.Identifier.Equals(itemId.Value));

            if (item is not null)
            {
                editorItem = item;
            }
        }

        var viewModel = new AppleMobileInterfaceCollectionEditorViewModel
        {
            WorkspaceId = WorkspaceId,
            InterfaceId = interfaceGuid,
            Item = editorItem
        };

        return PartialView("Partials/CollectionViewItemEditorPanel", viewModel);
    }

    [HttpPost("{interfaceId}/item-editor-panel/item/{itemId:guid?}")]
    public async Task<IActionResult> CollectionViewItemEditorPanelWithItem(string interfaceId, Guid? itemId,
        [FromForm] AppleMobileCollectionEditorPanelFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<AppleMobileCollectionViewJsonDataModel>();

        AppleMobileCollectionViewItemJsonDataModel? item = null;

        if (itemId.HasValue)
        {
            item = data.Items.FirstOrDefault(x => x.Identifier.Equals(itemId.Value));
        }

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            item ??= new AppleMobileCollectionViewItemJsonDataModel
            {
                Identifier = itemId ?? Guid.NewGuid()
            };
            item.Title = formModel.Title;
            item.ImageUrl = formModel.ImageUrl ?? string.Empty;
            item.Url = formModel.Url ?? string.Empty;
            return PartialView("Partials/CollectionViewItemEditorPanel", new AppleMobileInterfaceCollectionEditorViewModel
            {
                WorkspaceId = WorkspaceId,
                InterfaceId = interfaceGuid,
                Item = item
            });
        }

        if (item is null)
        {
            item = new AppleMobileCollectionViewItemJsonDataModel
            {
                Identifier = Guid.NewGuid()
            };

            data.Items = data.Items.Append(item).ToList();
        }

        item.Title = formModel.Title;
        item.ImageUrl = formModel.ImageUrl ?? string.Empty;
        item.Url = formModel.Url ?? string.Empty;

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Partials/CollectionViewItemEditorPanel", new AppleMobileInterfaceCollectionEditorViewModel
        {
            WorkspaceId = WorkspaceId,
            InterfaceId = interfaceGuid,
            Item = item
        });
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

        var data = contentNode.Config.Deserialize<AppleMobileCollectionViewJsonDataModel>();
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

        var viewModel = new AppleMobileInterfaceCollectionWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = data
        };

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Workspace", viewModel);
    }
}
