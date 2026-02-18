using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Controllers;
using FastGooey.Database;
using FastGooey.Features.Widgets.Clock.Models.FormModels;
using FastGooey.Features.Widgets.Clock.Models.JsonDataModels;
using FastGooey.Features.Widgets.Clock.Models.ViewModels.Clock;
using FastGooey.Features.Widgets.Map.Models.ViewModels.Map;
using FastGooey.Models;
using FastGooey.Services;
using FastGooey.Utils;
using Flurl.Http;
using MapKit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Widgets.Clock.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Widgets/Clock")]
public class ClockController(
    ILogger<ClockController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : 
    BaseStudioController(keyValueService, dbContext)
{
    private async Task<ClockWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var data = contentNode.Config.Deserialize<ClockJsonDataModel>();

        var currentTime =
            TimeFromCoordinates.CalculateDateTimeSet(data.Latitude, data.Longitude);

        var viewModel = new ClockWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = data,
            CurrentTime = currentTime
        };

        return viewModel;
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        var viewModel = new ClockViewModel
        {
            WorkspaceViewModel = workspaceViewModel
        };

        return View("Index", viewModel);
    }

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

    [HttpPost("create-widget")]
    public async Task<IActionResult> CreateWidget()
    {
        var workspace = GetWorkspace();
        var data = new ClockJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "Widget",
            ViewType = "Clock",
            Name = "New Clock Widget",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(contentNode.DocId);
        var viewModel = new ClockViewModel
        {
            WorkspaceViewModel = workspaceViewModel
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index", viewModel);
    }

    [HttpPost("workspace/{interfaceId}")]
    public async Task<IActionResult> SaveWorkspace(string interfaceId, [FromForm] ClockFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
            return PartialView("Partials/SearchPanel", new ClockSearchPanelViewModel
            {
                SearchText = formModel.Location,
                Results = Array.Empty<MapKitSearchResponseModelWithTime>()
            });
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<ClockJsonDataModel>();
        data.Location = formModel.Location;
        data.Latitude = formModel.Latitude;
        data.Longitude = formModel.Longitude;
        data.Coordinates = formModel.Coordinates;
        data.Timezone = formModel.Timezone;
        data.MapIdentifier = formModel.MapIdentifier;

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        return PartialView("Workspace", viewModel);
    }

    [HttpGet("search-panel")]
    public async Task<IActionResult> SearchPanel([FromQuery] string location)
    {
        var mapKitServerToken = await keyValueService.GetValueForKey(Constants.MapKitServerKey);

        var results = await $"https://maps-api.apple.com/v1/search?q={location}"
            .WithHeader("Authorization", $"Bearer {mapKitServerToken}")
            .GetJsonAsync<MapKitSearchResponseModel>();

        var resultsWithTime = (results.Results ?? [])
            .Where(x =>
                x.Coordinate.Latitude.HasValue &&
                x.Coordinate.Longitude.HasValue)
            .Select(x => new MapKitSearchResponseModelWithTime
            {
                Result = x,
                LocalDateTimeSet = TimeFromCoordinates.CalculateDateTimeSet(x.Coordinate.Latitude.Value, x.Coordinate.Longitude.Value)
            });

        var viewModel = new ClockSearchPanelViewModel
        {
            SearchText = location,
            Results = resultsWithTime
        };

        return PartialView("Partials/SearchPanel", viewModel);
    }

    [HttpGet("preview-panel/{interfaceId}")]
    public async Task<IActionResult> PreviewPanel(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        var viewModel = new ClockPreviewPanelViewModel
        {
            PreviewAvailable = !string.IsNullOrEmpty(workspaceViewModel.Data.Timezone),
            Date = workspaceViewModel.CurrentTime.LocalDate,
            Time = workspaceViewModel.CurrentTime.LocalTime,
            Location = workspaceViewModel.Data.Location,
        };

        return PartialView("Partials/PreviewPanel", viewModel);
    }
}
