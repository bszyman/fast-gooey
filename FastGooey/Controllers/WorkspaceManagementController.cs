using FastGooey.Database;
using FastGooey.Models.FormModels;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

[Route("Workspaces/{workspaceId:guid}/[controller]")]
public class WorkspaceManagementController(
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext): 
    BaseStudioController(keyValueService)
{
    [HttpGet]
    public IActionResult Index()
    {
        var workspace = dbContext.Workspaces
            .First(x => x.PublicId == WorkspaceId);

        var formModel = new WorkspaceManagementModel
        {
            WorkspaceName = workspace.Name,
        };
        
        var viewModel = new ManageWorkspaceViewModel
        {
            Workspace = workspace,
            FormModel = formModel
        };
        
        return View(viewModel);
    }
    
    [HttpGet("workspace")]
    public IActionResult OrganizationWorkspace()
    {
        var workspace = dbContext.Workspaces
            .First(x => x.PublicId == WorkspaceId);

        var formModel = new WorkspaceManagementModel
        {
            WorkspaceName = workspace.Name,
        };
        
        var viewModel = new ManageWorkspaceViewModel
        {
            Workspace = workspace,
            FormModel = formModel
        };
        
        return PartialView("~/Views/WorkspaceManagement/Workspaces/WorkspaceManagement.cshtml", viewModel);
    }
    
    [HttpPost("workspace/save")]
    public IActionResult EditWorkspace([Bind(Prefix = "FormModel")] WorkspaceManagementModel model)
    {
        var workspace = dbContext.Workspaces
            .First(x => x.PublicId == WorkspaceId);
        workspace.Name = model.WorkspaceName;

        dbContext.SaveChanges();

        var formModel = new WorkspaceManagementModel
        {
            WorkspaceName = workspace.Name,
        };
        
        var viewModel = new ManageWorkspaceViewModel
        {
            Workspace = workspace,
            FormModel = formModel
        };
        
        return PartialView("~/Views/WorkspaceManagement/Workspaces/WorkspaceManagement.cshtml", viewModel);
    }
}