using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.JsonDataModels.Mac;
using FastGooey.Models.ViewModels.Mac;
using FastGooey.Services;
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
    ApplicationDbContext dbContext): 
    BaseStudioController(keyValueService, dbContext)
{
    private async Task<MacInterfaceSourceListWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));
        
        var viewModel = new MacInterfaceSourceListWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = contentNode.Config.Deserialize<MacSourceListJsonDataModel>()
        };

        return viewModel;
    }
    
    [HttpPost("create-interface")]
    public async Task<IActionResult> CreateInterface()
    {
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
        
        Response.Headers.Append("HX-Trigger", "refreshNavigation");
        
        return PartialView("~/Views/MacSourceList/Index.cshtml", viewModel);
    }
    
    [HttpGet("{interfaceId:guid}")]
    public async Task<IActionResult> Index(Guid interfaceId)
    {
        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        var viewModel = new MacInterfaceSourceListViewModel
        {
            Workspace = workspaceViewModel
        };
        
        return View(viewModel);
    }
    
    [HttpGet("workspace/{interfaceId:guid}")]
    public async Task<IActionResult> Workspace(Guid interfaceId)
    {
        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        
        return PartialView("~/Views/MacSourceList/Workspace.cshtml", viewModel);
    }
    
    [HttpGet("{interfaceId:guid}/group-editor-panel/{groupId:guid?}")]
    public async Task<IActionResult> GroupEditorPanel(Guid interfaceId, Guid? groupId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

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
    
    [HttpPost("{interfaceId:guid}/group-editor-panel/{groupId:guid?}")]
    public async Task<IActionResult> SaveGroupEditorPanel(Guid interfaceId, Guid? groupId, [FromForm] MacSourceListGroupPanelFormModel formModel)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

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
    
    [HttpGet("{interfaceId:guid}/item-editor-panel/group/{groupId:guid}/item/{itemId:guid?}")]
    public async Task<IActionResult> ItemEditorPanel(Guid interfaceId, Guid groupId, Guid? itemId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var data = contentNode.Config.Deserialize<MacSourceListJsonDataModel>();
        var group = data?.Groups.FirstOrDefault(x => x.Identifier.Equals(groupId));

        if (group == null)
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
            Url = item.Url,
        };
        
        return PartialView("~/Views/MacSourceList/Partials/SourceListItemEditorPanel.cshtml", viewModel);    
    }

    [HttpPost("{interfaceId:guid}/item-editor-panel/group/{groupId:guid}/item/{itemId:guid?}")]
    public async Task<IActionResult> SaveItemEditorPanel(Guid interfaceId, Guid groupId, Guid? itemId,
        [FromForm] MacSourceListGroupItemPanelFormModel formModel)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var data = contentNode.Config.Deserialize<MacSourceListJsonDataModel>();
        var group = data?.Groups.FirstOrDefault(x => x.Identifier.Equals(groupId));

        if (group == null)
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
            Url = item.Url,
        };
        
        return PartialView("~/Views/MacSourceList/Partials/SourceListItemEditorPanel.cshtml", viewModel);
    }
    
    [HttpDelete("{interfaceId:guid}/group-editor-panel/group/{groupId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid interfaceId, Guid groupId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var data = contentNode.Config.Deserialize<MacSourceListJsonDataModel>();
        var group = data?.Groups.FirstOrDefault(x => x.Identifier.Equals(groupId));

        if (group == null)
            return BadRequest();
        
        data.Groups.Remove(group);
        
        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");
        return Ok();
    }
    
    [HttpDelete("{interfaceId:guid}/item-editor-panel/group/{groupId:guid}/item/{itemId:guid?}")]
    public async Task<IActionResult> DeleteItem(Guid interfaceId, Guid groupId, Guid? itemId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var data = contentNode.Config.Deserialize<MacSourceListJsonDataModel>();
        var group = data?.Groups.FirstOrDefault(x => x.Identifier.Equals(groupId));

        if (group == null)
            return BadRequest();
        
        var item = group.GroupItems.FirstOrDefault(x => x.Identifier.Equals(itemId.Value));
        
        if (item == null)
            return BadRequest();
        
        group.GroupItems.Remove(item);
        
        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");
        return Ok();
    }
}