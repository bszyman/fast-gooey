using System.Globalization;
using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Controllers;
using FastGooey.Database;
using FastGooey.Features.Widgets.Weather.Models.FormModels;
using FastGooey.Features.Widgets.Weather.Models.JsonDataModels;
using FastGooey.Features.Widgets.Weather.Models.ViewModels.Weather;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Utils;
using Flurl;
using Flurl.Http;
using GeoTimeZone;
using MapKit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Widgets.Weather.Controllers;

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

        return View("Weather", viewModel);
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

        return PartialView("Weather", viewModel);
    }

    [HttpPost("Workspace/{interfaceId}")]
    public async Task<IActionResult> SaveWeatherWorkspace(string interfaceId, [FromForm] WeatherWorkspaceFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
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

        return PartialView("Workspace", viewModel);
    }

    [HttpPost("search-panel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WeatherSearchPanel([FromForm] WeatherWorkspaceFormModel formModel)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return PartialView("Partials/WeatherSearchPanel", new WeatherSearchPanelViewModel
            {
                SearchText = formModel.Location,
                Results = new MapKitSearchResponseModel { Results = [] }
            });
        }

        var mapKitServerToken = await keyValueService.GetValueForKey(Constants.MapKitServerKey);

        var results = await $"https://maps-api.apple.com/v1/search?q={formModel.Location}"
            .WithHeader("Authorization", $"Bearer {mapKitServerToken}")
            .GetJsonAsync<MapKitSearchResponseModel>();

        var viewModel = new WeatherSearchPanelViewModel
        {
            SearchText = formModel.Location,
            Results = results
        };

        return PartialView("Partials/WeatherSearchPanel", viewModel);
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

        var config = gooeyInterface.Config.Deserialize<WeatherJsonDataModel>();
        var viewModel = new WeatherPreviewPanelViewModel
        {
            Location = config.Location
        };

        if (string.IsNullOrWhiteSpace(config.Latitude) || string.IsNullOrWhiteSpace(config.Longitude))
        {
            return PartialView("Partials/WeatherPreviewPanel", viewModel);
        }

        if (!double.TryParse(config.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
            !double.TryParse(config.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            return PartialView("Partials/WeatherPreviewPanel", viewModel);
        }

        var weatherKitJwt = await keyValueService.GetValueForKey(Constants.WeatherKitJwt);
        if (string.IsNullOrWhiteSpace(weatherKitJwt))
        {
            logger.LogWarning("WeatherKit JWT not found for weather preview.");
            return PartialView("Partials/WeatherPreviewPanel", viewModel);
        }

        try
        {
            var timezoneId = TimeZoneLookup.GetTimeZone(latitude, longitude).Result;

            var weatherData = await $"https://weatherkit.apple.com/api/v1/weather/en/{latitude}/{longitude}"
                .SetQueryParams(new
                {
                    timezone = timezoneId,
                    dataSets = "currentWeather"
                })
                .WithHeader("Authorization", $"Bearer {weatherKitJwt}")
                .GetJsonAsync<WeatherKit.Models.Weather>();

            if (weatherData?.CurrentWeather is not null)
            {
                var tempFahrenheit = (weatherData.CurrentWeather.Temperature * 9 / 5) + 32;
                viewModel.Temperature = Math.Round(tempFahrenheit).ToString(CultureInfo.InvariantCulture);
                viewModel.PreviewAvailable = true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to fetch WeatherKit preview data.");
        }

        return PartialView("Partials/WeatherPreviewPanel", viewModel);
    }
}
