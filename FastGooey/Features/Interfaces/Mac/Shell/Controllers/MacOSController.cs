using FastGooey.Attributes;
using FastGooey.Controllers;
using FastGooey.Database;
using FastGooey.Models.ViewModels;
using FastGooey.Models.ViewModels.NavigationBar;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Interfaces.Mac.Shell.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/MacOS")]
public class MacOSController(
    ILogger<MacOSController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var workspace = await dbContext.Workspaces.FirstAsync(
            x => x.PublicId.Equals(WorkspaceId)
        );
        
        var viewModel = new MacOSIndexViewModel
        {
            Workspace = workspace,
            NavBarViewModel = new MetalNavBarViewModel
            {
                WorkspaceName = workspace.Name,
                WorkspaceId = workspace.PublicId,
                ActiveTab = "macOS"
            }
        };
        
        return View(viewModel);
    }

    [HttpGet("interface-selector")]
    public async Task<IActionResult> MacInterfaceSelector(Guid workspaceId)
    {
        var macOSInterfaces = await GetInterfacesForWorkspace(workspaceId);
     
        var viewModel = new MacInterfaceSelectorViewModel
        {
            WorkspaceId = workspaceId,
            InterfaceItems = macOSInterfaces
        };
        
        return PartialView("Partials/MacInterfaceSelector", viewModel);
    }
    
    [HttpGet("mac-interface-create-palette")]
    public async Task<IActionResult> MacInterfaceCreatorPalette(Guid workspaceId)
    {
        if (await InterfaceLimitReachedAsync())
        {
            return PartialView("~/Views/Partials/UpgradeToStandardPanel.cshtml");
        }

        return PartialView("Partials/InterfaceCreatorPalette", workspaceId);
    }

    [HttpDelete("interface/{interfaceId:guid}")]
    public async Task<IActionResult> DeleteInterface(Guid workspaceId, Guid interfaceId)
    {
        var interfaceNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstOrDefaultAsync(x =>
                x.Workspace.PublicId.Equals(workspaceId) &&
                x.DocId.Equals(interfaceId) &&
                x.Platform.Equals("Mac"));

        if (interfaceNode is null)
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
            .Where(x => x.Platform.Equals("Mac"))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new InterfaceNavigationItem
            {
                Id = x.DocId,
                Name = x.Name,
                Type = x.ViewType,
                Route = $"/Workspaces/{workspaceId}/interfaces/mac/{x.ViewType}/{x.DocId.ToBase64Url()}"
            })
            .ToListAsync();

        return interfaces;
    }
}
