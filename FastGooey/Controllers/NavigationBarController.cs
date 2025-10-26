using FastGooey.Database;
using FastGooey.Models.ViewModels.NavigationBar;
using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers;

[Route("Workspaces/{workspaceId:guid}/NavigationBar")]
public class NavigationBarController(
    ILogger<NavigationBarController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext): 
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
        var weatherWidgets = await dbContext.GooeyInterfaces
            .Where(x => x.Workspace.PublicId.Equals(workspaceId))
            .Where(x => x.Platform.Equals("Widget"))
            .Select(x => new WidgetNavigationItem
            {
                Id = x.DocId,
                Name = x.Name,
                Type = x.ViewType,
                Route = $"/Workspaces/{workspaceId}/Widgets/{x.ViewType}/{x.DocId}"
            })
            .ToListAsync();

        return weatherWidgets;
    }
    
    private async Task<List<WidgetNavigationItem>> GetAppleMobileInterfacesForWorkspace(Guid workspaceId)
    {
        // TODO: Replace with actual database query
        // For now, returning mock data
        return new List<WidgetNavigationItem>
        {
            new() { Name = "Surf Spots", Route = $"/Workspaces/{workspaceId}/Interfaces/AppleMobile/List" },
            new() { Name = "Surf Spot Submission", Route = $"/Workspaces/{workspaceId}/Interfaces/AppleMobile/Form" },
            new() { Name = "10th Annual Event", Route = $"/Workspaces/{workspaceId}/Interfaces/AppleMobile/Content" }
        };
    }
    
    private async Task<List<WidgetNavigationItem>> GetMacOSInterfacesForWorkspace(Guid workspaceId)
    {
        // TODO: Replace with actual database query
        // For now, returning mock data
        return new List<WidgetNavigationItem>
        {
            new() { Name = "Surf Spots", Route = $"/Workspaces/{workspaceId}/Interfaces/MacOS/Table" },
            new() { Name = "Surf Spot Submission", Route = $"/Workspaces/{workspaceId}/Interfaces/MacOS/Form" },
            new() { Name = "10th Annual Event", Route = $"/Workspaces/{workspaceId}/Interfaces/MacOS/Content" },
            new() { Name = "Apple Hardware", Route = $"/Workspaces/{workspaceId}/Interfaces/MacOS/SourceList" },
            new() { Name = "eFoil Products", Route = $"/Workspaces/{workspaceId}/Interfaces/MacOS/Outline" }
        };
    }
    
    private async Task<List<WidgetNavigationItem>> GetTvOSInterfacesForWorkspace(Guid workspaceId)
    {
        // TODO: Replace with actual database query
        // For now, returning mock data
        return new List<WidgetNavigationItem>();
    }
}