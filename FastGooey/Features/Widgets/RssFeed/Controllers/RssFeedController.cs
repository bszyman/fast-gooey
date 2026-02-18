using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Xml;
using FastGooey.Attributes;
using FastGooey.Controllers;
using FastGooey.Database;
using FastGooey.Features.Widgets.RssFeed.Models.FormModels;
using FastGooey.Features.Widgets.RssFeed.Models.JsonDataModels;
using FastGooey.Features.Widgets.RssFeed.Models.ViewModels.RssFeed;
using FastGooey.Features.Widgets.Weather.Controllers;
using FastGooey.Models;
using FastGooey.Models.Response;
using FastGooey.Services;
using FastGooey.Utils;
using Flurl.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Features.Widgets.RssFeed.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Widgets/RssFeed")]
public class RssFeedController(
    ILogger<WeatherController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager
) : BaseStudioController(keyValueService, dbContext)
{
    private async Task<RssWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var viewModel = new RssWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = contentNode.Config.Deserialize<RssFeedJsonDataModel>()
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
        var viewModel = new RssViewModel
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
        var data = new RssFeedJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "Widget",
            ViewType = "RssFeed",
            Name = "New Rss Feed Widget",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(contentNode.DocId);
        var viewModel = new RssViewModel
        {
            WorkspaceViewModel = workspaceViewModel
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("Index", viewModel);
    }

    [HttpPost("workspace/{interfaceId}")]
    public async Task<IActionResult> SaveWorkspace(string interfaceId, [FromForm] RssFeedFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<RssFeedJsonDataModel>();
        data.FeedUrl = formModel.FeedUrl;

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        return PartialView("Workspace", viewModel);
    }

    [HttpGet("preview-panel/from-url")]
    public async Task<IActionResult> RssPreviewPanelFromUrl([FromQuery] string feedUrl)
    {
        if (string.IsNullOrWhiteSpace(feedUrl))
        {
            return BadRequest("RSS feed URL is required");
        }

        try
        {
            // Fetch the RSS feed using Flurl
            var stream = await feedUrl
                .WithTimeout(10)
                .GetStreamAsync();

            await using (stream)
            {
                using var xmlReader = XmlReader.Create(stream);

                // Parse the feed
                var feed = SyndicationFeed.Load(xmlReader);

                // Create a view model with the feed data
                var viewModel = new RssPreviewPanelViewModel
                {
                    FeedTitle = feed.Title?.Text,
                    FeedDescription = feed.Description?.Text,
                    FeedUrl = feedUrl,
                    Items = feed.Items.Take(10).Select(item => new RssFeedItem
                    {
                        Title = item.Title?.Text,
                        Summary = item.Summary?.Text,
                        Link = item.Links.FirstOrDefault()?.Uri?.ToString(),
                        PublishDate = item.PublishDate.DateTime
                    }).ToList()
                };

                return PartialView("Partials/PreviewPanel", viewModel);
            }
        }
        catch (FlurlHttpException ex)
        {
            logger.LogError(ex, "Failed to fetch RSS feed from {Url}", feedUrl);
            return BadRequest($"Failed to fetch RSS feed: {ex.Message}");
        }
        catch (XmlException ex)
        {
            logger.LogError(ex, "Failed to parse RSS feed from {Url}", feedUrl);
            return BadRequest("Invalid RSS feed format");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing RSS feed from {Url}", feedUrl);
            return StatusCode(500, "An error occurred while processing the RSS feed");
        }
    }

    [HttpGet("preview-panel/from-interface/{interfaceId}")]
    public async Task<IActionResult> RssPreviewPanel(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = dbContext.GooeyInterfaces.First(x => x.DocId.Equals(interfaceGuid));
        var data = contentNode.Config.Deserialize<RssFeedJsonDataModel>();

        if (string.IsNullOrWhiteSpace(data.FeedUrl))
        {
            return BadRequest("RSS feed URL is required");
        }

        try
        {
            // Fetch the RSS feed using Flurl
            var stream = await data.FeedUrl
                .WithTimeout(10)
                .GetStreamAsync();

            await using (stream)
            {
                using var xmlReader = XmlReader.Create(stream);

                // Parse the feed
                var feed = SyndicationFeed.Load(xmlReader);

                // Create a view model with the feed data
                var viewModel = new RssPreviewPanelViewModel
                {
                    FeedTitle = feed.Title?.Text,
                    FeedDescription = feed.Description?.Text,
                    FeedUrl = data.FeedUrl,
                    Items = feed.Items.Take(10).Select(item => new RssFeedItem
                    {
                        Title = item.Title?.Text,
                        Summary = item.Summary?.Text,
                        Link = item.Links.FirstOrDefault()?.Uri?.ToString(),
                        PublishDate = item.PublishDate.DateTime
                    }).ToList()
                };

                return PartialView("Partials/PreviewPanel", viewModel);
            }
        }
        catch (FlurlHttpException ex)
        {
            logger.LogError(ex, "Failed to fetch RSS feed from {Url}", data.FeedUrl);
            return BadRequest($"Failed to fetch RSS feed: {ex.Message}");
        }
        catch (XmlException ex)
        {
            logger.LogError(ex, "Failed to parse RSS feed from {Url}", data.FeedUrl);
            return BadRequest("Invalid RSS feed format");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing RSS feed from {Url}", data.FeedUrl);
            return StatusCode(500, "An error occurred while processing the RSS feed");
        }
    }
}
