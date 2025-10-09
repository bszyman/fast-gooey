using FastGooey.Models.ViewModels;
using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

public class WidgetsController(
    ILogger<WidgetsController> logger, 
    IKeyValueService keyValueService
    ): BaseStudioController(keyValueService)
{
    // Full page view
    public IActionResult Weather()
    {
        var viewModel = new WeatherViewModel
        {
            workspaceViewModel = new WeatherWorkspaceModel()
        };
        
        return View(viewModel);
    }
    
    // Workspace partial for HTMX
    public IActionResult WeatherWorkspace()
    {
        var viewModel = new WeatherWorkspaceModel();
        return View("~/Views/Widgets/Workspaces/Weather.cshtml", viewModel);
    }
    
    public IActionResult Map()
    {
        var viewModel = new MapViewModel
        {
            workspaceViewModel = new MapWorkspaceModel()
        };
        
        return View(viewModel);
    }
    
    public IActionResult MapWorkspace()
    {
        var viewModel = new MapWorkspaceModel();
        return View("~/Views/Widgets/Workspaces/Map.cshtml", viewModel);
    }
    
    public IActionResult Clock()
    {
        var viewModel = new ClockViewModel
        {
            workspaceViewModel = new ClockWorkspaceModel()
        };
        
        return View(viewModel);
    }
    
    public IActionResult ClockWorkspace()
    {
        var viewModel = new ClockWorkspaceModel();
        return View("~/Views/Widgets/Workspaces/Clock.cshtml", viewModel);
    }
    
    public IActionResult Rss()
    {
        var viewModel = new RssViewModel
        {
            workspaceViewModel = new RssWorkspaceModel()
        };
        
        return View(viewModel);
    }
    
    public IActionResult RssWorkspace()
    {
        var viewModel = new RssWorkspaceModel();
        return View("~/Views/Widgets/Workspaces/Rss.cshtml", viewModel);
    }
}