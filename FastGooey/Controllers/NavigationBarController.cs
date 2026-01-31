using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models.ViewModels.NavigationBar;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/NavigationBar")]
public class NavigationBarController(
    ILogger<NavigationBarController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] Guid workspaceId)
    {
        // Query all widgets for this workspace
        var widgets = await GetWidgetsForWorkspace(workspaceId);

        // Query all interfaces for this workspace
        var appleMobileInterfaces = await GetAppleMobileInterfacesForWorkspace(workspaceId);
        var macOSInterfaces = await GetMacOSInterfacesForWorkspace(workspaceId);
        var tvOSInterfaces = await GetTvOSInterfacesForWorkspace(workspaceId);

        var viewModel = new NavigationBarViewModel
        {
            WorkspaceId = workspaceId,
            Widgets = widgets,
            AppleMobileInterfaces = appleMobileInterfaces,
            MacOSInterfaces = macOSInterfaces,
            TvOSInterfaces = tvOSInterfaces
        };

        return PartialView("~/Views/NavigationBar/NavigationBar.cshtml", viewModel);
    }

    private async Task<List<WidgetNavigationItem>> GetWidgetsForWorkspace(Guid workspaceId)
    {
        var widgets = await dbContext.GooeyInterfaces
            .Where(x => x.Workspace.PublicId.Equals(workspaceId))
            .Where(x => x.Platform.Equals("Widget"))
            .Select(x => new WidgetNavigationItem
            {
                Id = x.DocId,
                Name = x.Name,
                Type = x.ViewType,
                Route = $"/Workspaces/{workspaceId}/Widgets/{x.ViewType}/{x.DocId.ToBase64Url()}"
            })
            .ToListAsync();

        return widgets;
    }

    private async Task<List<InterfaceNavigationItem>> GetAppleMobileInterfacesForWorkspace(Guid workspaceId)
    {
        var interfaces = await dbContext.GooeyInterfaces
            .Where(x => x.Workspace.PublicId.Equals(workspaceId))
            .Where(x => x.Platform.Equals("AppleMobile"))
            .Select(x => new InterfaceNavigationItem
            {
                Id = x.DocId,
                Name = x.Name,
                Type = x.ViewType,
                Route = $"/Workspaces/{workspaceId}/Interfaces/AppleMobile/{x.ViewType}/{x.DocId.ToBase64Url()}"
            })
            .ToListAsync();

        return interfaces;
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

    private async Task<List<InterfaceNavigationItem>> GetTvOSInterfacesForWorkspace(Guid workspaceId)
    {
        // TODO: Replace with actual database query
        // For now, returning mock data
        return new List<InterfaceNavigationItem>();
    }
}
