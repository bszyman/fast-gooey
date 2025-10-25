using System.Text.Json;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.JsonDataModels;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using Flurl.Http;
using MapKit.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers.Widgets;

[Route("Workspaces/{workspaceId:guid}/Widgets/Weather")]
public class WeatherController(
    ILogger<WeatherController> logger, 
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager
): BaseStudioController(keyValueService, dbContext)
{
    // Full page view
    [HttpGet("{interfaceId:guid}")]
    public async Task<IActionResult> Weather(Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));
        
        var viewModel = new WeatherViewModel
        {
            workspaceViewModel = new WeatherWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = contentNode.Config.Deserialize<WeatherJsonDataModel>()
            }
        };
        
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWeatherWidget()
    {
        var workspace = GetWorkspace();
        var data = new WeatherJsonDataModel();
        
        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "widget",
            ViewType = "weather",
            Name = "New Weather Widget",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var viewModel = new WeatherViewModel
        {
            workspaceViewModel = new WeatherWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };
        
        Response.Headers.Append("HX-Trigger", "refreshNavigation");
        
        return PartialView("~/Views/Weather/Weather.cshtml", viewModel);
    }
    
    // Workspace partial for HTMX
    [HttpGet("Workspace/{interfaceId:guid}")]
    public async Task<IActionResult> WeatherWorkspace(Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));
        
        var viewModel = new WeatherWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = contentNode.Config.Deserialize<WeatherJsonDataModel>()
        };
        
        return PartialView("~/Views/Weather/Workspace.cshtml", viewModel);
    }
    
    [HttpPost("Workspace/{interfaceId:guid}")]
    public async Task<IActionResult> SaveWeatherWorkspace(Guid interfaceId, [FromForm] WeatherWorkspaceFormModel formModel)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }
        
        var gooeyInterface = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var docData = gooeyInterface.Config.Deserialize<WeatherJsonDataModel>();
        docData.Location = formModel.Location;
        docData.Latitude = formModel.Latitude;
        docData.Longitude = formModel.Longitude;
        docData.Coordinates = formModel.Coordinates;
        
        gooeyInterface.Config = JsonSerializer.SerializeToDocument(docData);

        await dbContext.SaveChangesAsync();
        
        var viewModel = new WeatherWorkspaceViewModel
        {
            ContentNode = gooeyInterface,
            Data = docData
        };
        
        return PartialView("~/Views/Weather/Workspace.cshtml", viewModel);
    }

    [HttpGet("SearchPanel")]
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

    [HttpGet("PreviewPanel/{interfaceId:guid}")]
    public async Task<IActionResult> WeatherPreviewPanel(Guid interfaceId)
    {
        var gooeyInterface = await dbContext.GooeyInterfaces
            .FirstAsync(x => x.DocId.Equals(interfaceId));
        
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