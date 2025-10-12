using System.ServiceModel.Syndication;
using System.Xml;
using FastGooey.Models.Response;
using FastGooey.Models.ViewModels;
using FastGooey.Services;
using FastGooey.Utils;
using Flurl.Http;
using MapKit.Models;
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
        return PartialView("~/Views/Widgets/Workspaces/Weather.cshtml", viewModel);
    }

    public async Task<IActionResult> WeatherSearchPanel([FromQuery] string city)
    {
        var mapKitServerToken = await keyValueService.GetValueForKey(Constants.MapKitServerKey);
        
        var results = await $"https://maps-api.apple.com/v1/search?q={city}"
            .WithHeader("Authorization", $"Bearer {mapKitServerToken}")
            .GetJsonAsync<MapKitSearchResponseModel>();

        var viewModel = new WeatherSearchPanelViewModel
        {
            SearchText = city,
            Results = results
        };
        
        return PartialView("~/Views/Widgets/Partials/WeatherSearchPanel.cshtml", viewModel);
    }
    
    public IActionResult Map()
    {
        var workspaceViewModel = new MapWorkspaceModel();
        
        workspaceViewModel.Entries.Add(new MapCityEntryViewModel
        {
            Latitude = 30.40728,
            Longitude = -87.21936,
            LocationName = "Pensacola, Florida, United States",
            LocationIdentifier = "pns-fl-usa",
            Index = 0,
            CoordinateDisplay = MapKitCoordinateModel.CoordinateDisplay(30.40728, -87.21936)
        });
        
        var viewModel = new MapViewModel
        {
            workspaceViewModel = workspaceViewModel
        };
        
        return View(viewModel);
    }
    
    public IActionResult MapWorkspace()
    {
        var viewModel = new MapWorkspaceModel();
        
        viewModel.Entries.Add(new MapCityEntryViewModel
        {
            Latitude = 30.40728,
            Longitude = -87.21936,
            LocationName = "Pensacola, Florida, United States",
            LocationIdentifier = "pns-fl-usa",
            Index = 0,
            CoordinateDisplay = MapKitCoordinateModel.CoordinateDisplay(30.40728, -87.21936)
        });
        
        return PartialView("~/Views/Widgets/Workspaces/Map.cshtml", viewModel);
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
        
        return PartialView("~/Views/Widgets/Partials/MapSearchPanel.cshtml", viewModel);
    }

    [HttpPost]
    public IActionResult CityEntry(double latitude, double longitude, string locationName, string locationIdentifier, int index)
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
        
        return PartialView("~/Views/Widgets/Partials/CityEntry.cshtml", viewModel);
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
        return PartialView("~/Views/Widgets/Workspaces/Clock.cshtml", viewModel);
    }

    public async Task<IActionResult> ClockSearchPanel([FromQuery] string city)
    {
        var mapKitServerToken = await keyValueService.GetValueForKey(Constants.MapKitServerKey);
        
        var results = await $"https://maps-api.apple.com/v1/search?q={city}"
            .WithHeader("Authorization", $"Bearer {mapKitServerToken}")
            .GetJsonAsync<MapKitSearchResponseModel>();

        var resultsWithTime = results.Results.Select(x => new MapKitSearchResponseModelWithTime
        {
            Result = x,
            LocalDateTimeSet = TimeFromCoordinates.CalculateDateTimeSet(x.Coordinate.Latitude.Value, x.Coordinate.Longitude.Value)
        });

        var viewModel = new ClockSearchPanelViewModel
        {
            SearchText = city,
            Results = resultsWithTime
        };
        
        return PartialView("~/Views/Widgets/Partials/ClockSearchPanel.cshtml", viewModel);
    }
    
    public IActionResult Rss()
    {
        var rssViewModel = new RssPreviewPanelViewModel();
        var workspaceViewModel = new RssWorkspaceModel
        {
            PreviewPanelViewModel = rssViewModel
        };
        
        var viewModel = new RssViewModel
        {
            WorkspaceViewModel = workspaceViewModel
        };
        
        return View(viewModel);
    }
    
    public IActionResult RssWorkspace()
    {
        var rssViewModel = new RssPreviewPanelViewModel();
        
        var viewModel = new RssWorkspaceModel
        {
            PreviewPanelViewModel = rssViewModel
        };
        
        return PartialView("~/Views/Widgets/Workspaces/Rss.cshtml", viewModel);
    }

    public async Task<IActionResult> RssPreviewPanel(string feedUrl)
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
                
                return PartialView("~/Views/Widgets/Partials/RssPreviewPanel.cshtml", viewModel);
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
}