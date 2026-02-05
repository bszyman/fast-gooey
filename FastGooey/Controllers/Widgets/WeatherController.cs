using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.JsonDataModels;
using FastGooey.Models.ViewModels.Weather;
using FastGooey.Services;
using FastGooey.Utils;
using Flurl.Http;
using MapKit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers.Widgets;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Widgets/Weather")]
public class WeatherController(
    ILogger<WeatherController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager ): 
    BaseStudioController(keyValueService, dbContext)
{
    private async Task<WeatherWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var viewModel = new WeatherWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = contentNode.Config.Deserialize<WeatherJsonDataModel>()
        };

        return viewModel;
    }

    // Full page view
    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Weather(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        var viewModel = new WeatherViewModel
        {
            WorkspaceViewModel = workspaceViewModel
        };

        return View(viewModel);
    }

    // Workspace partial for HTMX
    [HttpGet("Workspace/{interfaceId}")]
    public async Task<IActionResult> WeatherWorkspace(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        return PartialView("~/Views/Weather/Workspace.cshtml", viewModel);
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
            ViewType = "Weather",
            Name = "New Weather Widget",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(contentNode.DocId);
        var viewModel = new WeatherViewModel
        {
            WorkspaceViewModel = workspaceViewModel
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("~/Views/Weather/Weather.cshtml", viewModel);
    }

    [HttpPost("Workspace/{interfaceId}")]
    public async Task<IActionResult> SaveWeatherWorkspace(string interfaceId, [FromForm] WeatherWorkspaceFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var gooeyInterface = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var docData = gooeyInterface.Config.Deserialize<WeatherJsonDataModel>();
        docData.Location = formModel.Location;
        docData.Latitude = formModel.Latitude;
        docData.Longitude = formModel.Longitude;
        docData.Coordinates = formModel.Coordinates;

        gooeyInterface.Config = JsonSerializer.SerializeToDocument(docData);

        await dbContext.SaveChangesAsync();

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        return PartialView("~/Views/Weather/Workspace.cshtml", viewModel);
    }

    [HttpGet("search-panel")]
    public async Task<IActionResult> WeatherSearchPanel([FromQuery] string location)
    {
        var mapKitServerToken = await keyValueService.GetValueForKey(Constants.MapKitServerKey);

        var results = await $"https://maps-api.apple.com/v1/search?q={location}"
            .WithHeader("Authorization", $"Bearer {mapKitServerToken}")
            .GetJsonAsync<MapKitSearchResponseModel>();

        var viewModel = new WeatherSearchPanelViewModel
        {
            SearchText = location,
            Results = results
        };

        return PartialView("~/Views/Weather/Partials/WeatherSearchPanel.cshtml", viewModel);
    }

    [HttpGet("preview-panel/{interfaceId}")]
    public async Task<IActionResult> WeatherPreviewPanel(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var gooeyInterface = await dbContext.GooeyInterfaces
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        // var config = gooeyInterface.Config.Deserialize<WeatherJsonDataModel>();
        // var weatherKitKey = keyValueService.GetValueForKey("");

        // fetch from weatherkit
        // var weatherDataRequest =
        //     await $"https://weatherkit.apple.com/api/v1/weather/en/{config.Latitude}/{config.Longitude}?timezone=America/Chicago&dataSets=currentWeather,forecastDaily&dailyEnd={endDateAsString}Z"
        //         .WithHeader("Authorization", $"Bearer {authorizationToken}")
        //         .GetJsonAsync<WeatherKitResponseModel>();

        var viewModel = new WeatherPreviewPanelViewModel
        {
            Temperature = "79",
            Location = "Pensacola, FL",
        };

        return PartialView("~/Views/Weather/Partials/WeatherPreviewPanel.cshtml", viewModel);
    }
}
