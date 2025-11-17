using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.JsonDataModels;
using FastGooey.Models.ViewModels.Clock;
using FastGooey.Models.ViewModels.Map;
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
[Route("Workspaces/{workspaceId:guid}/Widgets/Clock")]
public class ClockController(
    ILogger<WeatherController> logger, 
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager
): BaseStudioController(keyValueService, dbContext)
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
    
    [HttpGet("{interfaceId:guid}")]
    public async Task<IActionResult> Index(Guid interfaceId)
    {
        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        var viewModel = new ClockViewModel
        {
            WorkspaceViewModel = workspaceViewModel
        };
        
        return View(viewModel);
    }
    
    [HttpGet("workspace/{interfaceId:guid}")]
    public async Task<IActionResult> Workspace(Guid interfaceId)
    {
        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        
        return PartialView("~/Views/Clock/Workspace.cshtml", viewModel);
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
        
        Response.Headers.Append("HX-Trigger", "refreshNavigation");
        
        return PartialView("~/Views/Clock/Index.cshtml", viewModel);
    }

    [HttpPost("workspace/{interfaceId:guid}")]
    public async Task<IActionResult> SaveWorkspace(Guid interfaceId, [FromForm] ClockFormModel formModel)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var data = contentNode.Config.Deserialize<ClockJsonDataModel>();
        data.Location = formModel.Location;
        data.Latitude = formModel.Latitude;
        data.Longitude = formModel.Longitude;
        data.Coordinates = formModel.Coordinates;
        data.Timezone = formModel.Timezone;
        data.MapIdentifier = formModel.MapIdentifier;
        
        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();
        
        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        
        return PartialView("~/Views/Clock/Workspace.cshtml", viewModel);
    }

    [HttpGet("search-panel")]
    public async Task<IActionResult> SearchPanel([FromQuery] string location)
    {
        var mapKitServerToken = await keyValueService.GetValueForKey(Constants.MapKitServerKey);
        
        var results = await $"https://maps-api.apple.com/v1/search?q={location}"
            .WithHeader("Authorization", $"Bearer {mapKitServerToken}")
            .GetJsonAsync<MapKitSearchResponseModel>();

        var resultsWithTime = results.Results
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
        
        return PartialView("~/Views/Clock/Partials/SearchPanel.cshtml", viewModel);
    }
    
    [HttpGet("preview-panel/{interfaceId:guid}")]
    public async Task<IActionResult> PreviewPanel(Guid interfaceId)
    {
        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        var viewModel = new ClockPreviewPanelViewModel
        {
            PreviewAvailable = !string.IsNullOrEmpty(workspaceViewModel.Data.Timezone),
            Date = workspaceViewModel.CurrentTime.LocalDate,
            Time = workspaceViewModel.CurrentTime.LocalTime,
            Location = workspaceViewModel.Data.Location,
        };
        
        return PartialView("~/Views/Clock/Partials/PreviewPanel.cshtml", viewModel);
    }
}