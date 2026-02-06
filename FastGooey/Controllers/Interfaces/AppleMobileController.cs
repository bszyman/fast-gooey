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
[Route("Workspaces/{workspaceId:guid}/Interfaces/iOS")]
public class AppleMobileController(
    ILogger<MacOSController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var workspace = await dbContext.Workspaces.FirstAsync(x => x.PublicId.Equals(WorkspaceId));
        
        var viewModel = new AppleMobileIndexViewModel
        {
            Workspace = workspace,
            NavBarViewModel = new MetalNavBarViewModel
            {
                WorkspaceName = workspace.Name,
                WorkspaceId = workspace.PublicId,
                ActiveTab = "iOS"
            }
        };
        
        return View(viewModel);
    }
    
    [HttpGet("interface-selector")]
    public async Task<IActionResult> MacInterfaceSelector(Guid workspaceId)
    {
        var iOSInterfaces = await GetInterfacesForWorkspace(workspaceId);
     
        var viewModel = new AppleMobileInterfaceSelectorViewModel
        {
            WorkspaceId = workspaceId,
            InterfaceItems = iOSInterfaces
        };
        
        return PartialView("~/Views/AppleMobile/Partials/InterfaceSelector.cshtml", viewModel);
    }
    
    [HttpGet("new-interface-panel")]
    public async Task<IActionResult> MacInterfaceSelectorPanel(Guid workspaceId)
    {
        if (await InterfaceLimitReachedAsync())
        {
            return PartialView("~/Views/Workspaces/Partials/UpgradeToStandardPanel.cshtml");
        }

        return PartialView("~/Views/AppleMobile/Partials/NewInterfaceSelectorPanel.cshtml", workspaceId);
    }
    
    private async Task<List<InterfaceNavigationItem>> GetInterfacesForWorkspace(Guid workspaceId)
    {
        var interfaces = await dbContext.GooeyInterfaces
            .Where(x => x.Workspace.PublicId.Equals(workspaceId))
            .Where(x => x.Platform.Equals("AppleMobile"))
            .Select(x => new InterfaceNavigationItem
            {
                Id = x.DocId,
                Name = x.Name,
                Type = x.ViewType,
                Route = $"/Workspaces/{workspaceId}/interfaces/AppleMobile/{x.ViewType}/{x.DocId.ToBase64Url()}"
            })
            .ToListAsync();

        return interfaces;
    }
}