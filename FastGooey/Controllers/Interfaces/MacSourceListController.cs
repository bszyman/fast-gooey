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
[Route("Workspaces/{workspaceId:guid}/interfaces/mac/sourcelist")]
public class MacSourceListController(
    ILogger<MacSourceListController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private async Task<MacInterfaceSourceListWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<MacInterfaceSourceListWorkspaceViewModel, MacSourceListJsonDataModel>(interfaceId);
    }

    [HttpPost("create-interface")]
    public async Task<IActionResult> CreateInterface()
    {
        if (await InterfaceLimitReachedAsync())
        {
            return Forbid();
        }

        var workspace = GetWorkspace();
        var data = new MacSourceListJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "Mac",
            ViewType = "SourceList",
            Name = "New Source List Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var viewModel = new MacInterfaceSourceListViewModel
        {
            Workspace = new MacInterfaceSourceListWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("~/Views/MacSourceList/Index.cshtml", viewModel);
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        var viewModel = new MacInterfaceSourceListViewModel
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

        return PartialView("~/Views/MacSourceList/Workspace.cshtml", viewModel);
    }

    [HttpGet("{interfaceId}/group-editor-panel/{groupId:guid?}")]
    public async Task<IActionResult> GroupEditorPanel(string interfaceId, Guid? groupId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacSourceListJsonDataModel>();

        var group = groupId.HasValue
            ? data?.Groups.FirstOrDefault(x => x.Identifier.Equals(groupId.Value)) ?? new MacSourceListGroupJsonDataModel()
            : new MacSourceListGroupJsonDataModel();

        var viewModel = new MacInterfaceSourceListGroupEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            GroupId = group.Identifier,
            GroupName = group.GroupName
        };

        return PartialView("~/Views/MacSourceList/Partials/SourceListEditorPanel.cshtml", viewModel);
    }

    [HttpPost("{interfaceId}/group-editor-panel/{groupId:guid?}")]
    public async Task<IActionResult> SaveGroupEditorPanel(string interfaceId, Guid? groupId, [FromForm] MacSourceListGroupPanelFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacSourceListJsonDataModel>();

        var group = groupId.HasValue && groupId.Value != Guid.Empty
            ? data?.Groups.FirstOrDefault(x => x.Identifier.Equals(groupId.Value)) ?? new MacSourceListGroupJsonDataModel()
            : new MacSourceListGroupJsonDataModel();

        if (ModelState.IsValid)
        {
            if (group.Identifier.Equals(Guid.Empty))
            {
                group.Identifier = Guid.NewGuid();
                data.Groups.Add(group);
            }

            group.GroupName = formModel.GroupName;

            contentNode.Config = JsonSerializer.SerializeToDocument(data);
            await dbContext.SaveChangesAsync();

            Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");
        }

        var viewModel = new MacInterfaceSourceListGroupEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            GroupId = group.Identifier,
            GroupName = group.GroupName
        };

        return PartialView("~/Views/MacSourceList/Partials/SourceListEditorPanel.cshtml", viewModel);
    }

    [HttpGet("{interfaceId}/item-editor-panel/group/{groupId:guid}/item/{itemId:guid?}")]
    public async Task<IActionResult> ItemEditorPanel(string interfaceId, Guid groupId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacSourceListJsonDataModel>();
        var group = data?.Groups.FirstOrDefault(x => x.Identifier.Equals(groupId));

        if (group is null)
            return BadRequest();

        var item = itemId.HasValue && itemId.Value != Guid.Empty ?
            group.GroupItems.FirstOrDefault(x => x.Identifier.Equals(itemId.Value)) ?? new MacSourceListGroupItemJsonDataModel()
            : new MacSourceListGroupItemJsonDataModel();

        var viewModel = new MacInterfaceSourceListGroupItemEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            GroupId = group.Identifier,
            Identifier = item.Identifier,
            Title = item.Title,
            Icon = item.Icon,
            Url = item.Url,
        };

        return PartialView("~/Views/MacSourceList/Partials/SourceListItemEditorPanel.cshtml", viewModel);
    }

    [HttpPost("{interfaceId}/item-editor-panel/group/{groupId:guid}/item/{itemId:guid?}")]
    public async Task<IActionResult> SaveItemEditorPanel(string interfaceId, Guid groupId, Guid? itemId,
        [FromForm] MacSourceListGroupItemPanelFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacSourceListJsonDataModel>();
        var group = data?.Groups.FirstOrDefault(x => x.Identifier.Equals(groupId));

        if (group is null)
            return BadRequest();

        var item = itemId.HasValue && itemId != Guid.Empty ?
            group.GroupItems.FirstOrDefault(x => x.Identifier.Equals(itemId.Value)) ?? new MacSourceListGroupItemJsonDataModel()
                : new MacSourceListGroupItemJsonDataModel();

        if (ModelState.IsValid)
        {
            if (item.Identifier.Equals(Guid.Empty))
            {
                item.Identifier = Guid.NewGuid();
                group.GroupItems.Add(item);
            }

            item.Title = formModel.Title;
            item.Icon = formModel.Icon ?? string.Empty;
            item.Url = formModel.Url;

            contentNode.Config = JsonSerializer.SerializeToDocument(data);
            await dbContext.SaveChangesAsync();

            Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");
        }

        var viewModel = new MacInterfaceSourceListGroupItemEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            GroupId = group.Identifier,
            Identifier = item.Identifier,
            Title = item.Title,
            Icon = item.Icon,
            Url = item.Url,
        };

        return PartialView("~/Views/MacSourceList/Partials/SourceListItemEditorPanel.cshtml", viewModel);
    }

    [HttpDelete("{interfaceId}/group-editor-panel/group/{groupId:guid}")]
    public async Task<IActionResult> DeleteItem(string interfaceId, Guid groupId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacSourceListJsonDataModel>();
        var group = data?.Groups.FirstOrDefault(x => x.Identifier.Equals(groupId));

        if (group is null)
            return BadRequest();

        data.Groups.Remove(group);

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");
        return Ok();
    }

    [HttpDelete("{interfaceId}/item-editor-panel/group/{groupId:guid}/item/{itemId:guid?}")]
    public async Task<IActionResult> DeleteItem(string interfaceId, Guid groupId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacSourceListJsonDataModel>();
        var group = data?.Groups.FirstOrDefault(x => x.Identifier.Equals(groupId));

        if (group is null)
            return BadRequest();

        var item = group.GroupItems.FirstOrDefault(x => x.Identifier.Equals(itemId.Value));

        if (item is null)
            return BadRequest();

        group.GroupItems.Remove(item);

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");
        return Ok();
    }
}
