using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models.ViewModels;
using FastGooey.Models.ViewModels.NavigationBar;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers.Interfaces;

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
        var viewModel = new MacOSIndexViewModel
        {
            WorkspaceId = WorkspaceId
        };
        
        return View(viewModel);
    }

    [HttpGet("interface-selector")]
    public async Task<IActionResult> MacInterfaceSelector(Guid workspaceId)
    {
        var macOSInterfaces = await GetMacOSInterfacesForWorkspace(workspaceId);
     
        var viewModel = new MacInterfaceSelectorViewModel
        {
            WorkspaceId = workspaceId,
            InterfaceItems = macOSInterfaces
        };
        
        return PartialView("~/Views/MacOS/Partials/MacInterfaceSelector.cshtml", viewModel);
    }
    
    [HttpGet("mac-interface-selector-panel")]
    public async Task<IActionResult> MacInterfaceSelectorPanel(Guid workspaceId)
    {
        // TODO: move views to appropriate paths 
        if (await InterfaceLimitReachedAsync())
        {
            return PartialView("~/Views/Workspaces/Partials/UpgradeToStandardPanel.cshtml");
        }

        return PartialView("~/Views/Workspaces/Partials/MacInterfaceSelectorPanel.cshtml", workspaceId);
    }
    
    private async Task<List<InterfaceNavigationItem>> GetMacOSInterfacesForWorkspace(Guid workspaceId)
    {
        var interfaces = await dbContext.GooeyInterfaces
            .Where(x => x.Workspace.PublicId.Equals(workspaceId))
            .Where(x => x.Platform.Equals("Mac"))
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