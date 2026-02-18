using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Controllers;
using FastGooey.Database;
using FastGooey.Features.Widgets.Map.Models.FormModels;
using FastGooey.Features.Widgets.Map.Models.JsonDataModels;
using FastGooey.Features.Widgets.Map.Models.ViewModels.Map;
using FastGooey.Features.Widgets.Weather.Controllers;
using FastGooey.Features.Widgets.Weather.Models.JsonDataModels;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Utils;
using Flurl.Http;
using MapKit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Widgets.Map.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Widgets/Map")]
public class MapController(
    ILogger<WeatherController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager ) : 
    BaseStudioController(keyValueService, dbContext)
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
                CoordinateDisplay = x.Coordinates,
                EntryId = x.EntryId
            });

        var viewModel = new MapWorkspaceViewModel
        {
            ContentNode = contentNode,
            Entries = entries
        };

        return viewModel;
    }

    // Full page view
    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        var viewModel = new MapViewModel
        {
            WorkspaceViewModel = workspaceViewModel
        };

        return View("Index", viewModel);
    }

    // Workspace view
    [HttpGet("workspace/{interfaceId}")]
    public async Task<IActionResult> Workspace(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        return PartialView("Workspace", viewModel);
    }

    [HttpPost("workspace/{interfaceId}")]
    public async Task<IActionResult> SaveWorkspace(string interfaceId, [FromForm] MapWorkspaceFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MapJsonDataModel>();

        data.Pins = formModel.Locations.Select(x => new MapWorkspacePinModel
        {
            Coordinates = x.Coordinates,
            EntryId = x.EntryId,
            Latitude = x.Latitude.ToString("G"),
            Longitude = x.Longitude.ToString("G"),
            LocationIdentifier = string.Empty,
            LocationName = x.LocationName
        }).ToList();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        return PartialView("Workspace", viewModel);
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
            WorkspaceViewModel = workspaceViewModel
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index", viewModel);
    }

    [HttpGet("search-panel")]
    public async Task<IActionResult> MapSearchPanel([FromQuery] string locationSearch)
    {
        var mapKitServerToken = await keyValueService.GetValueForKey(Constants.MapKitServerKey);

        var results = await $"https://maps-api.apple.com/v1/search?q={locationSearch}"
            .WithHeader("Authorization", $"Bearer {mapKitServerToken}")
            .GetJsonAsync<MapKitSearchResponseModel>();

        var viewModel = new MapSearchPanelViewModel
        {
            WorkspaceId = WorkspaceId,
            SearchText = locationSearch,
            Results = results
        };

        return PartialView("Partials/SearchPanel", viewModel);
    }

    [HttpPost("add-location")]
    [ValidateAntiForgeryToken]
    public IActionResult AddLocationEntry([FromForm] MapAddLocationEntryFormModel formModel)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            var panelViewModel = new MapSearchPanelViewModel
            {
                WorkspaceId = WorkspaceId,
                Results = new MapKitSearchResponseModel
                {
                    Results = []
                }
            };

            return PartialView("Partials/SearchPanel", panelViewModel);
        }
        
        var viewModel = new MapCityEntryViewModel
        {
            Latitude = formModel.Latitude!.Value,
            Longitude = formModel.Longitude!.Value,
            LocationName = formModel.LocationName!,
            LocationIdentifier = formModel.LocationIdentifier!,
            EntryId = Guid.NewGuid(),
            CoordinateDisplay = MapKitCoordinateModel.CoordinateDisplay(formModel.Latitude.Value, formModel.Longitude.Value)
        };

        return PartialView("Partials/CityEntry", viewModel);
    }
}
