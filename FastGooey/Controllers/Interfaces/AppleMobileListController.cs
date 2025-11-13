using System.Text.Json;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.JsonDataModels;
using FastGooey.Models.ViewModels.AppleMobileInterface;
using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers.Interfaces;

[Route("Workspaces/{workspaceId:guid}/Interfaces/AppleMobile/List")]
public class AppleMobileListController(
    ILogger<AppleMobileListController> logger, 
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext): 
    BaseStudioController(keyValueService, dbContext)
{
    private async Task<AppleMobileInterfaceListWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));
        
        var viewModel = new AppleMobileInterfaceListWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = contentNode.Config.Deserialize<AppleMobileListJsonDataModel>()
        };

        return viewModel;
    }
    
    [HttpGet("{interfaceId:guid}")]
    public async Task<IActionResult> Index(Guid interfaceId)
    {
        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        var viewModel = new AppleMobileInterfaceListViewModel
        {
            WorkspaceViewModel = workspaceViewModel
        };
        
        return View(viewModel);
    }
    
    [HttpGet("workspace/{interfaceId:guid}")]
    public async Task <IActionResult> ListWorkspace(Guid interfaceId)
    {
        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        
        return PartialView("~/Views/AppleMobileList/Workspace.cshtml", viewModel);
    }
    
    [HttpPost("create-interface")]
    public async Task<IActionResult> CreateInterface()
    {
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
        
        Response.Headers.Append("HX-Trigger", "refreshNavigation");
        
        return PartialView("~/Views/AppleMobileList/Index.cshtml", viewModel);
    }

    [HttpGet("{interfaceId:guid}/item-editor-panel/item/{itemId:guid?}")]
    public IActionResult ListItemEditorPanel(Guid interfaceId, Guid? itemId)
    {
        var editorItem = new AppleMobileListItemJsonDataModel();

        if (itemId.HasValue)
        {
            var contentNode = dbContext.GooeyInterfaces
                .First(x => x.DocId.Equals(interfaceId));
            var data = contentNode.Config.Deserialize<AppleMobileListJsonDataModel>();
            
            var item = data.Items
                .FirstOrDefault(x => x.Identifier.Equals(itemId.Value));
            
            if (item != null)
            {
                editorItem = item;
            }
        }
        
        var viewModel = new AppleMobileInterfaceListEditorViewModel
        {
            WorkspaceId = WorkspaceId,
            InterfaceId = interfaceId,
            Item = editorItem
        };
        
        return PartialView("~/Views/AppleMobileList/Partials/ListItemEditorPanel.cshtml", viewModel);
    }
    
    [HttpPost("{interfaceId:guid}/item-editor-panel/item/{itemId:guid?}")]
    public async Task<IActionResult> ListItemEditorPanelWithItem(Guid interfaceId, Guid? itemId, [FromForm] AppleMobileListEditorPanelFormModel formModel)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));
        
        var data = contentNode.Config.Deserialize<AppleMobileListJsonDataModel>();

        AppleMobileListItemJsonDataModel? item = null;

        if (itemId.HasValue)
        {
            item = data.Items.FirstOrDefault(x => x.Identifier.Equals(itemId.Value));
        }

        if (ModelState.IsValid)
        {
            if (item == null)
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
            InterfaceId = interfaceId,
            Item = item
        };
        
        return PartialView("~/Views/AppleMobileList/Partials/ListItemEditorPanel.cshtml", viewModel);
    }
    
    [HttpDelete("{interfaceId:guid}/item/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid interfaceId, Guid itemId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));
        
        var data = contentNode.Config.Deserialize<AppleMobileListJsonDataModel>();
        var item = data.Items.FirstOrDefault(x => x.Identifier.Equals(itemId));

        if (item == null)
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
        
        return PartialView("~/Views/AppleMobileList/Workspace.cshtml", viewModel);
    }
}