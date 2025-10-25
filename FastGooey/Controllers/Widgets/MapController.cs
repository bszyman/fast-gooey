using System.Text.Json;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.JsonDataModels;
using FastGooey.Models.ViewModels;
using FastGooey.Models.ViewModels.Map;
using FastGooey.Services;
using Flurl.Http;
using MapKit.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers.Widgets;

[Route("Workspaces/{workspaceId:guid}/Widgets/Map")]
public class MapController(
    ILogger<WeatherController> logger, 
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager
): BaseStudioController(keyValueService, dbContext)
{
    private async Task<MapWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var entries = contentNode.Config.Deserialize<MapJsonDataModel>()
            .Pins
            .Select(x => new MapCityEntryViewModel 
            { 
                LocationName = x.LocationName,
                LocationIdentifier = x.LocationIdentifier,
                Latitude = double.Parse(x.Latitude),
                Longitude = double.Parse(x.Longitude),
                CoordinateDisplay = x.Coordinates
            });
        
        var viewModel = new MapWorkspaceViewModel
        {
            ContentNode = contentNode,
            Entries = entries
        };

        return viewModel;
    }
    
    // Full page view
    [HttpGet("{interfaceId:guid}")]
    public async Task<IActionResult> Index(Guid interfaceId)
    {
        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        
        var viewModel = new MapViewModel
        {
            workspaceViewModel = workspaceViewModel
        };
        
        return View(viewModel);
    }
    
    // Workspace view
    [HttpGet("workspace/{interfaceId:guid}")]
    public async Task<IActionResult> Workspace(Guid interfaceId)
    {
        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        
        return PartialView(viewModel);
    }
    
    [HttpPost("create-widget")]
    public async Task<IActionResult> CreateWidget()
    {
        var workspace = GetWorkspace();
        var data = new WeatherJsonDataModel();
        
        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "Widget",
            ViewType = "Map",
            Name = "New Map Widget",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(contentNode.DocId);
        var viewModel = new MapViewModel
        {
            workspaceViewModel = workspaceViewModel
        };
        
        Response.Headers.Append("HX-Trigger", "refreshNavigation");
        
        return PartialView("~/Views/Map/Index.cshtml", viewModel);
    }
    
    public async Task<IActionResult> MapSearchPanel([FromQuery] string locationSearch)
    {
        var mapKitServerToken = await keyValueService.GetValueForKey(Constants.MapKitServerKey);
        
        var results = await $"https://maps-api.apple.com/v1/search?q={locationSearch}"
            .WithHeader("Authorization", $"Bearer {mapKitServerToken}")
            .GetJsonAsync<MapKitSearchResponseModel>();

        var viewModel = new MapSearchPanelViewModel
        {
            SearchText = locationSearch,
            Results = results
        };
        
        return PartialView("~/Views/Map/Partials/SearchPanel.cshtml", viewModel);
    }

    [HttpPost("add-location")]
    public IActionResult AddLocationEntry(double latitude, double longitude, string locationName, string locationIdentifier, int index)
    {
        var viewModel = new MapCityEntryViewModel
        {
            Latitude = latitude,
            Longitude = longitude,
            LocationName = locationName,
            LocationIdentifier = locationIdentifier,
            Index = index,
            CoordinateDisplay = MapKitCoordinateModel.CoordinateDisplay(latitude, longitude)
        };
        
        return PartialView("~/Views/Map/Partials/CityEntry.cshtml", viewModel);
    }
}