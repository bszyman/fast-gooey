using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.JsonDataModels.Mac;
using FastGooey.Models.ViewModels.Mac;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers.Interfaces;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/interfaces/mac/outline")]
public class MacOutlineController(
    ILogger<MacOutlineController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private async Task<MacOutlineWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<MacOutlineWorkspaceViewModel, MacOutlineJsonDataModel>(interfaceId);
    }

    [HttpPost("create-interface")]
    public async Task<IActionResult> CreateInterface()
    {
        if (await InterfaceLimitReachedAsync())
        {
            return Forbid();
        }

        var workspace = GetWorkspace();
        var data = new MacOutlineJsonDataModel
        {
            Identifier = Guid.NewGuid(),
            Name = "Root",
            Children = []
        };

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "Mac",
            ViewType = "Outline",
            Name = "New Outline Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var viewModel = new MacOutlineViewModel
        {
            Workspace = new MacOutlineWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        Response.Headers.Append("HX-Trigger", "refreshNavigation");

        return PartialView("~/Views/MacOutline/Index.cshtml", viewModel);
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        var viewModel = new MacOutlineViewModel
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

        return PartialView("~/Views/MacOutline/Workspace.cshtml", viewModel);
    }

    [HttpGet("{interfaceId}/editor-panel/{itemId:guid}")]
    public async Task<IActionResult> EditorPanel(string interfaceId, Guid itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacOutlineJsonDataModel>();
        var item = data.FindById(itemId);

        var viewModel = new MacOutlineEditorPanelViewModel
        {
            WorkspaceId = WorkspaceId,
            InterfaceId = interfaceGuid,
            Name = item.Name,
            Identifier = item.Identifier,
            Url = item.Url,
        };

        return PartialView("~/Views/MacOutline/Partials/OutlineViewItemEditorPanel.cshtml", viewModel);
    }

    [HttpGet("{interfaceId}/editor-panel/{parentId:guid}/new")]
    public async Task<IActionResult> CreateEditorPanel(string interfaceId, Guid parentId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacOutlineJsonDataModel>();
        var parentItem = data.FindById(parentId);

        var viewModel = new MacOutlineEditorPanelViewModel
        {
            WorkspaceId = WorkspaceId,
            InterfaceId = interfaceGuid,
            ParentId = parentItem.Identifier.ToString(),
            ParentName = parentItem.Name,
        };

        return PartialView("~/Views/MacOutline/Partials/OutlineViewItemEditorPanel.cshtml", viewModel);
    }

    [HttpPost("{interfaceId}/editor-panel/{itemId:guid?}")]
    public async Task<IActionResult> SaveEditorPanel(
        string interfaceId,
        Guid? itemId,
        [FromForm] MacOutlineEditorPanelFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacOutlineJsonDataModel>();

        MacOutlineJsonDataModel? item;

        if (itemId.HasValue)
        {
            // Update existing
            item = data?.FindById(itemId.Value);
            if (item is null)
            {
                // Item to update not found in tree
                return NotFound();
            }
        }
        else
        {
            // Create new
            if (!formModel.ParentId.HasValue || formModel.ParentId == Guid.Empty)
            {
                return BadRequest("ParentId is required when creating a new item.");
            }

            var parentItem = data.FindById(formModel.ParentId.Value);
            if (parentItem is null)
            {
                return NotFound("Parent item not found.");
            }

            item = new MacOutlineJsonDataModel
            {
                Identifier = Guid.NewGuid()
            };

            parentItem.Children.Add(item);
        }

        if (ModelState.IsValid)
        {
            item.Name = formModel.Name;
            item.Url = formModel.Url;

            contentNode.Config = JsonSerializer.SerializeToDocument(data);
            await dbContext.SaveChangesAsync();

            Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");
        }

        var viewModel = new MacOutlineEditorPanelViewModel
        {
            Name = item.Name,
            Identifier = item.Identifier,
            Url = item.Url,
        };

        return PartialView("~/Views/MacOutline/Partials/OutlineViewItemEditorPanel.cshtml", viewModel);
    }

    [HttpDelete("{interfaceId}/editor-panel/{itemId:guid?}")]
    public async Task<IActionResult> DeleteItem(string interfaceId, Guid? itemId)
    {
        if (!itemId.HasValue)
        {
            return BadRequest("itemId is required.");
        }

        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacOutlineJsonDataModel>();
        if (data is null)
        {
            return NotFound("Outline data not found.");
        }

        // Find the list that contains the item
        if (!data.TryFindParentListFor(itemId.Value, out var parentList))
        {
            return NotFound();
        }

        if (parentList is null)
        {
            // Item is the root node – depending on your rules, either clear it or forbid delete
            return BadRequest("Cannot delete the root item.");
        }

        var index = parentList.FindIndex(x => x.Identifier == itemId.Value);
        if (index < 0)
        {
            return NotFound();
        }

        parentList.RemoveAt(index);

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace");
        return Ok();
    }
}
