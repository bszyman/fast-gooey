using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.ViewModels;
using FastGooey.Models.ViewModels.NavigationBar;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Widgets")]
public class WidgetsController(
    ILogger<WidgetsController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager): 
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var workspace = await dbContext.Workspaces.FirstAsync(
            x => x.PublicId.Equals(WorkspaceId)
        );
        
        var viewModel = new WidgetsIndexViewModel
        {
            Workspace = workspace,
            NavBarViewModel = new MetalNavBarViewModel
            {
                WorkspaceName = workspace.Name,
                WorkspaceId = workspace.PublicId,
                ActiveTab = "Widgets"
            }
        };
        
        return View(viewModel);
    }
    
    [HttpGet("interface-selector")]
    public async Task<IActionResult> MacInterfaceSelector(Guid workspaceId)
    {
        var widgets = await GetInterfacesForWorkspace(workspaceId);
     
        var viewModel = new WidgetsInterfaceSelectorViewModel
        {
            WorkspaceId = workspaceId,
            InterfaceItems = widgets
        };
        
        return PartialView("~/Views/Widgets/Partials/WidgetsInterfaceSelector.cshtml", viewModel);
    }
    
    [HttpGet("new-interface-panel")]
    public async Task<IActionResult> NewInterfacePanel(Guid workspaceId)
    {
        if (await InterfaceLimitReachedAsync())
        {
            return PartialView("~/Views/Workspaces/Partials/UpgradeToStandardPanel.cshtml");
        }

        return PartialView("~/Views/Widgets/Partials/NewWidgetPartialView.cshtml", workspaceId);
    }

    [HttpDelete("interface/{interfaceId:guid}")]
    public async Task<IActionResult> DeleteInterface(Guid workspaceId, Guid interfaceId)
    {
        var interfaceNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstOrDefaultAsync(x =>
                x.Workspace.PublicId.Equals(workspaceId) &&
                x.DocId.Equals(interfaceId) &&
                x.Platform.Equals("Widget"));

        if (interfaceNode == null)
        {
            return NotFound();
        }

        dbContext.GooeyInterfaces.Remove(interfaceNode);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return Content(
            "<div class=\"flex justify-center items-center h-full w-full\">Select an interface to get started.</div>",
            "text/html"
        );
    }
    
    private async Task<List<InterfaceNavigationItem>> GetInterfacesForWorkspace(Guid workspaceId)
    {
        var interfaces = await dbContext.GooeyInterfaces
            .Where(x => x.Workspace.PublicId.Equals(workspaceId))
            .Where(x => x.Platform.Equals("Widget"))
            .Select(x => new InterfaceNavigationItem
            {
                Id = x.DocId,
                Name = x.Name,
                Type = x.ViewType,
                Route = $"/Workspaces/{workspaceId}/widgets/{x.ViewType}/{x.DocId.ToBase64Url()}"
            })
            .ToListAsync();

        return interfaces;
    }
}
