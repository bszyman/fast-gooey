using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Features.Interfaces.AppleTv.Product.Models;
using FastGooey.Features.Interfaces.Shared.Controllers;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.AppleTv.Product.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/tvOS/Product")]
public class AppleTvProductController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private async Task<AppleTvProductWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<AppleTvProductWorkspaceViewModel, AppleTvProductJsonDataModel>(interfaceId);
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = new AppleTvInterfaceProductViewModel
        {
            Workspace = await WorkspaceViewModelForInterfaceId(interfaceGuid)
        };

        return View(viewModel);
    }

    [HttpGet("workspace/{interfaceId}")]
    public async Task<IActionResult> Workspace(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        return PartialView("Workspace", viewModel);
    }

    [HttpPost("workspace/{interfaceId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveWorkspace(string interfaceId, [FromForm] ProductWorkspaceFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));
        var data = contentNode.Config.Deserialize<AppleTvProductJsonDataModel>() ?? new AppleTvProductJsonDataModel();

        data.Title = formModel.Title.Trim();
        data.Description = formModel.Description.Trim();
        data.PreviewMediaUrl = formModel.PreviewMediaUrl.Trim();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        var viewModel = new AppleTvProductWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = data
        };

        return PartialView("Workspace", viewModel);
    }

    [HttpGet("workspace/{interfaceId}/related-items/{relatedItemId?}")]
    public async Task<IActionResult> RelatedItemPanel(string interfaceId, Guid? relatedItemId)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));
        var data = contentNode.Config.Deserialize<AppleTvProductJsonDataModel>() ?? new AppleTvProductJsonDataModel();

        var relatedItem = relatedItemId.HasValue
            ? data.RelatedProducts.FirstOrDefault(x => x.Id.Equals(relatedItemId.Value)) ?? new AppleTvProductRelatedItemJsonModel()
            : new AppleTvProductRelatedItemJsonModel();

        return PartialView("Partials/RelatedItemPanel", new RelatedItemPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            RelatedItemId = relatedItem.Id,
            Title = relatedItem.Title,
            Link = relatedItem.Link,
            MediaUrl = relatedItem.MediaUrl
        });
    }

    [HttpPost("workspace/{interfaceId}/related-items/{relatedItemId?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveRelatedItemPanel(
        string interfaceId,
        Guid? relatedItemId,
        [FromForm] RelatedItemPanelFormModel formModel)
    {
        if (!TryParseInterfaceId(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));
        var data = contentNode.Config.Deserialize<AppleTvProductJsonDataModel>() ?? new AppleTvProductJsonDataModel();

        var item = relatedItemId.HasValue
            ? data.RelatedProducts.FirstOrDefault(x => x.Id.Equals(relatedItemId.Value))
            : null;

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return PartialView("Partials/RelatedItemPanel", new RelatedItemPanelViewModel
            {
                WorkspaceId = contentNode.Workspace.PublicId,
                InterfaceId = contentNode.DocId,
                RelatedItemId = item?.Id ?? relatedItemId ?? Guid.Empty,
                Title = formModel.Title,
                Link = formModel.Link,
                MediaUrl = formModel.MediaUrl
            });
        }

        if (item is null)
        {
            item = new AppleTvProductRelatedItemJsonModel
            {
                Id = Guid.NewGuid()
            };
            data.RelatedProducts.Add(item);
        }

        item.Title = formModel.Title.Trim();
        item.Link = formModel.Link.Trim();
        item.MediaUrl = formModel.MediaUrl.Trim();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");

        return PartialView("Partials/RelatedItemPanel", new RelatedItemPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            RelatedItemId = item.Id,
            Title = item.Title,
            Link = item.Link,
            MediaUrl = item.MediaUrl
        });
    }

    [HttpPost("create-interface")]
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

        var data = new AppleTvProductJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "Product",
            Name = "New Product Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        var viewModel = new AppleTvInterfaceProductViewModel
        {
            Workspace = new AppleTvProductWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        return PartialView("Index", viewModel);
    }
}
