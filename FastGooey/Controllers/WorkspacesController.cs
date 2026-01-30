using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}")]
public class WorkspacesController(
    ILogger<WorkspacesController> logger, 
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager): 
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet("Home")]
    public IActionResult Home(Guid workspaceId)
    {
        return View();
    }

    [HttpGet("Info/{interfaceId:guid}")]
    public IActionResult Info(Guid workspaceId, Guid interfaceId)
    {
        var interfaceNode = dbContext.GooeyInterfaces
            .First(x => x.DocId.Equals(interfaceId));
        
        var workspace = dbContext.Workspaces
            .First(x => x.PublicId == workspaceId);

        var viewModel = new InfoViewModel
        {
            ContentNode = interfaceNode,
            Workspace = workspace
        };
        
        return View(viewModel);
    }

    [HttpPost("Edit")]
    public IActionResult EditWorkspace(Guid workspaceId)
    {
        return View();
    }

    [HttpDelete("Delete")]
    public IActionResult DeleteWorkspace(Guid workspaceId)
    {
        return View();
    }
    
    [HttpPost("UpdateTitle/{interfaceId:guid}")]
    public async Task<IActionResult> UpdateTitle(Guid interfaceId, [FromForm] string title)
    {
        var interfaceNode = dbContext.GooeyInterfaces
            .First(x => x.DocId.Equals(interfaceId));
        
        interfaceNode.Name = title;
        await dbContext.SaveChangesAsync();
        
        Response.Headers.Append("HX-Trigger", "refreshNavigation");
        
        return Ok();
    }
    
    [HttpGet("apple-mobile-interface-selector-panel")]
    public async Task<IActionResult> AppleMobileInterfaceSelectorPanel(Guid workspaceId)
    {
        if (await InterfaceLimitReachedAsync())
        {
            return PartialView("~/Views/Workspaces/Partials/UpgradeToStandardPanel.cshtml");
        }

        return PartialView("~/Views/Workspaces/Partials/AppleMobileInterfaceSelectorPanel.cshtml", workspaceId);
    }
    
    [HttpGet("mac-interface-selector-panel")]
    public async Task<IActionResult> MacInterfaceSelectorPanel(Guid workspaceId)
    {
        if (await InterfaceLimitReachedAsync())
        {
            return PartialView("~/Views/Workspaces/Partials/UpgradeToStandardPanel.cshtml");
        }

        return PartialView("~/Views/Workspaces/Partials/MacInterfaceSelectorPanel.cshtml", workspaceId);
    }
}
