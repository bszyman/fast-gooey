using System.Text.Json;
using System.Text.Json.Nodes;
using System.Globalization;
using System.ServiceModel.Syndication;
using System.Xml;
using FastGooey.Database;
using FastGooey.Features.Interfaces.AppleTv.Alert.Models;
using FastGooey.Features.Interfaces.AppleTv.DescriptiveAlert.Models;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv.Accessories;
using FastGooey.Features.Interfaces.Mac.Shared.Models.JsonDataModels.Mac;
using FastGooey.Features.Widgets.Clock.Models.JsonDataModels;
using FastGooey.Features.Widgets.Map.Models.JsonDataModels;
using FastGooey.Features.Widgets.RssFeed.Models.JsonDataModels;
using FastGooey.Features.Widgets.Weather.Models.JsonDataModels;
using FastGooey.HypermediaResponses;
using FastGooey.Models;
using FastGooey.Models.JsonDataModels;
using FastGooey.Services;
using FastGooey.Utils;
using Flurl;
using Flurl.Http;
using GeoTimeZone;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WeatherKit.Models;

namespace FastGooey.Controllers;

[Route("hypermedia")]
public class HypermediaController(
    ApplicationDbContext dbContext,
    IMemoryCache memoryCache,
    IKeyValueService keyValueService,
    ILogger<HypermediaController> logger) : Controller
{
    private const string FastGooeyLinkScheme = "fastgooey:";
    private const string FastGooeyMediaScheme = "fastgooey:media:";
    private const int MaxRssArticles = 10;
    private const double CelsiusToFahrenheitMultiplier = 9d / 5d;
    private const double FahrenheitOffset = 32d;

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Get(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var cacheKey = $"hypermedia:{interfaceGuid}:{Request.Scheme}:{Request.Host}";
        if (memoryCache.TryGetValue(cacheKey, out IHypermediaResponse? cachedResponse))
        {
            return Ok(cachedResponse);
        }

        var contentNode = await dbContext.GooeyInterfaces
            .AsNoTracking()
            .Include(x => x.Workspace)
            .FirstOrDefaultAsync(x => x.DocId.Equals(interfaceGuid));

        if (contentNode is null) return NotFound();

        IHypermediaResponse? hypermediaResponse = null;

        if (contentNode.Platform.Equals("AppleMobile"))
        {
            hypermediaResponse = GenerateAppleMobileResponse(contentNode);
        }

        if (contentNode.Platform.Equals("Mac"))
        {
            hypermediaResponse = GenerateMacResponse(contentNode);
        }

        if (contentNode.Platform.Equals("AppleTv"))
        {
            hypermediaResponse = GenerateAppleTvResponse(contentNode);
        }

        if (contentNode.Platform.Equals("Widget"))
        {
            hypermediaResponse = await GenerateWidgetResponse(contentNode);
        }

        if (hypermediaResponse is null) return NotFound();

        var cacheDuration = GetCacheDuration(contentNode);
        memoryCache.Set(cacheKey, hypermediaResponse, cacheDuration);
        return Ok(hypermediaResponse);
    }

    private static TimeSpan GetCacheDuration(GooeyInterface gooeyInterface)
    {
        if (!gooeyInterface.Platform.Equals("Widget"))
        {
            return TimeSpan.FromSeconds(10);
        }

        return gooeyInterface.ViewType switch
        {
            "Clock" => TimeSpan.FromSeconds(1),
            "RssFeed" => TimeSpan.FromMinutes(5),
            "Weather" => TimeSpan.FromMinutes(5),
            _ => TimeSpan.FromSeconds(10)
        };
    }

    private IHypermediaResponse GenerateAppleMobileResponse(GooeyInterface gooeyInterface)
    {
        switch (gooeyInterface.ViewType)
        {
            case "List":
                return GenerateAppleMobileList(gooeyInterface);
            case "Collection":
                return GenerateAppleMobileCollection(gooeyInterface);
            case "Content":
                return GenerateAppleMobileContent(gooeyInterface);
            default:
                return NotSupported();
        }
    }

    private IHypermediaResponse GenerateMacResponse(GooeyInterface gooeyInterface)
    {
        switch (gooeyInterface.ViewType)
        {
            case "Table":
                return GenerateMacTable(gooeyInterface);
            case "Collection":
                return GenerateMacCollection(gooeyInterface);
            case "SourceList":
                return GenerateMacSourceList(gooeyInterface);
            case "Content":
                return GenerateMacContent(gooeyInterface);
            case "Outline":
                return GenerateMacOutline(gooeyInterface);
            default:
                return NotSupported();
        }
    }

    private async Task<IHypermediaResponse> GenerateWidgetResponse(GooeyInterface gooeyInterface)
    {
        switch (gooeyInterface.ViewType)
        {
            case "Clock":
                return GenerateClockResponse(gooeyInterface);
            case "Map":
                return GenerateMapResponse(gooeyInterface);
            case "RssFeed":
                return await GenerateRssFeedResponse(gooeyInterface);
            case "Weather":
                return await GenerateWeatherResponse(gooeyInterface);
            default:
                return NotSupported();
        }
    }

    private IHypermediaResponse GenerateAppleTvResponse(GooeyInterface gooeyInterface)
    {
        switch (gooeyInterface.ViewType)
        {
            case "Main":
                return GenerateAppleTvMain(gooeyInterface);
            case "List":
                return GenerateAppleTvList(gooeyInterface);
            case "Alert":
                return GenerateAppleTvAlert(gooeyInterface);
            case "DescriptiveAlert":
                return GenerateAppleTvDescriptiveAlert(gooeyInterface);
            default:
                return NotSupported();
        }
    }

    private AppleTvMainHypermediaResponse GenerateAppleTvMain(GooeyInterface gooeyInterface)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var config = gooeyInterface.Config.Deserialize<MainJsonDataModel>(options) ?? new MainJsonDataModel();
        var menuBarButtons = config.MenuBarButtons
            .Select(x => new NavigationButtonJsonDataModel
            {
                Text = x.Text,
                Link = UnfurlFastGooeyLink(x.Link, gooeyInterface.Workspace.PublicId)
            }).ToList();

        return new AppleTvMainHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            BackgroundSplash = new BackgroundSplash
            {
                ImageResource = UnfurlFastGooeyLink(config.BackgroundSplash.ImageResource, gooeyInterface.Workspace.PublicId),
                AudioResource = UnfurlFastGooeyLink(config.BackgroundSplash.AudioResource, gooeyInterface.Workspace.PublicId)
            },
            MenuBarButtons = menuBarButtons
        };
    }

    private AppleTvListHypermediaResponse GenerateAppleTvList(GooeyInterface gooeyInterface)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var config = gooeyInterface.Config.Deserialize<ListJsonDataModel>(options) ?? new ListJsonDataModel();
        var configNode = JsonSerializer.SerializeToNode(config);
        var unfurledConfig = UnfurlFastGooeyLinksAndReturn(configNode, gooeyInterface.Workspace.PublicId)?
            .Deserialize<ListJsonDataModel>(options) ?? new ListJsonDataModel();

        return new AppleTvListHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Banner = unfurledConfig.Banner,
            Header = unfurledConfig.Header,
            ListItems = unfurledConfig.ListItems
        };
    }

    private AppleTvAlertHypermediaResponse GenerateAppleTvAlert(GooeyInterface gooeyInterface)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var config = gooeyInterface.Config.Deserialize<AlertContentJsonDataModel>(options) ?? new AlertContentJsonDataModel();
        var configNode = JsonSerializer.SerializeToNode(config);
        var unfurledConfig = UnfurlFastGooeyLinksAndReturn(configNode, gooeyInterface.Workspace.PublicId)?
            .Deserialize<AlertContentJsonDataModel>(options) ?? new AlertContentJsonDataModel();

        return new AppleTvAlertHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Title = unfurledConfig.Title,
            Description = unfurledConfig.Description,
            UpperButtonText = unfurledConfig.UpperButtonText,
            UpperButtonLink = unfurledConfig.UpperButtonLink,
            LowerButtonText = unfurledConfig.LowerButtonText,
            LowerButtonLink = unfurledConfig.LowerButtonLink
        };
    }

    private AppleTvDescriptiveAlertHypermediaResponse GenerateAppleTvDescriptiveAlert(GooeyInterface gooeyInterface)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var config = gooeyInterface.Config.Deserialize<DescriptiveAlertContentJsonDataModel>(options) ??
                     new DescriptiveAlertContentJsonDataModel();
        var configNode = JsonSerializer.SerializeToNode(config);
        var unfurledConfig = UnfurlFastGooeyLinksAndReturn(configNode, gooeyInterface.Workspace.PublicId)?
            .Deserialize<DescriptiveAlertContentJsonDataModel>(options) ?? new DescriptiveAlertContentJsonDataModel();

        return new AppleTvDescriptiveAlertHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Title = unfurledConfig.Title,
            CancelButtonText = unfurledConfig.CancelButtonText,
            ConfirmButtonText = unfurledConfig.ConfirmButtonText,
            DescriptiveContent = unfurledConfig.DescriptiveContent
        };
    }

    private NotSupported NotSupported()
    {
        return new NotSupported();
    }

    private WidgetClockHypermediaResponse GenerateClockResponse(GooeyInterface gooeyInterface)
    {
        var config = gooeyInterface.Config.Deserialize<ClockJsonDataModel>() ?? new ClockJsonDataModel();
        var dateTimeSet = TimeFromCoordinates.CalculateDateTimeSet(config.Latitude, config.Longitude);

        return new WidgetClockHypermediaResponse(config, dateTimeSet)
        {
            InterfaceId = gooeyInterface.DocId
        };
    }

    private WidgetMapHypermediaResponse GenerateMapResponse(GooeyInterface gooeyInterface)
    {
        var config = gooeyInterface.Config.Deserialize<MapJsonDataModel>() ?? new MapJsonDataModel();
        return new WidgetMapHypermediaResponse(config)
        {
            InterfaceId = gooeyInterface.DocId
        };
    }

    private async Task<WidgetRssFeedHypermediaResponse> GenerateRssFeedResponse(GooeyInterface gooeyInterface)
    {
        var config = gooeyInterface.Config.Deserialize<RssFeedJsonDataModel>() ?? new RssFeedJsonDataModel();
        var response = new WidgetRssFeedHypermediaResponse(config)
        {
            InterfaceId = gooeyInterface.DocId
        };

        if (!Uri.TryCreate(config.FeedUrl, UriKind.Absolute, out var feedUri) ||
            (feedUri.Scheme != Uri.UriSchemeHttp && feedUri.Scheme != Uri.UriSchemeHttps))
        {
            return response;
        }

        try
        {
            var stream = await feedUri.ToString()
                .WithTimeout(10)
                .GetStreamAsync();

            await using (stream)
            {
                using var xmlReader = XmlReader.Create(stream);
                var feed = SyndicationFeed.Load(xmlReader);
                response.FeedTitle = feed?.Title?.Text ?? string.Empty;
                response.Articles = feed?.Items
                    .Take(MaxRssArticles)
                    .Select(item => new WidgetRssFeedItemResponse
                    {
                        Title = item.Title?.Text ?? string.Empty,
                        Summary = item.Summary?.Text ?? string.Empty,
                        Link = item.Links.FirstOrDefault()?.Uri?.ToString() ?? string.Empty,
                        PublishDate = item.PublishDate == DateTimeOffset.MinValue ? null : item.PublishDate.UtcDateTime
                    })
                    .ToList() ?? [];
            }
        }
        catch (FlurlHttpException ex)
        {
            logger.LogWarning(ex, "Failed to fetch RSS feed for interface {InterfaceId}", gooeyInterface.DocId);
        }
        catch (XmlException ex)
        {
            logger.LogWarning(ex, "Failed to parse RSS feed for interface {InterfaceId}", gooeyInterface.DocId);
        }

        return response;
    }

    private async Task<WidgetWeatherHypermediaResponse> GenerateWeatherResponse(GooeyInterface gooeyInterface)
    {
        var config = gooeyInterface.Config.Deserialize<WeatherJsonDataModel>() ?? new WeatherJsonDataModel();
        var response = new WidgetWeatherHypermediaResponse(config)
        {
            InterfaceId = gooeyInterface.DocId
        };

        if (!double.TryParse(config.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
            !double.TryParse(config.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            return response;
        }

        var weatherKitJwt = await keyValueService.GetValueForKey(Constants.WeatherKitJwt);
        if (string.IsNullOrWhiteSpace(weatherKitJwt))
        {
            return response;
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
                .GetJsonAsync<Weather>();

            if (weatherData?.CurrentWeather is not null)
            {
                var tempFahrenheit = (weatherData.CurrentWeather.Temperature * CelsiusToFahrenheitMultiplier) + FahrenheitOffset;
                response.Temperature = Math.Round(tempFahrenheit).ToString(CultureInfo.InvariantCulture);
                response.ConditionCode = weatherData.CurrentWeather.ConditionCode ?? string.Empty;
                response.PreviewAvailable = true;
            }
        }
        catch (FlurlHttpException ex)
        {
            logger.LogWarning(ex, "Failed to fetch weather data for interface {InterfaceId}", gooeyInterface.DocId);
        }

        return response;
    }

    private AppleMobileListHypermediaResponse GenerateAppleMobileList(GooeyInterface gooeyInterface)
    {
        var content = gooeyInterface.Config.Deserialize<AppleMobileListJsonDataModel>();
        var listData = content?.Items
            .Select(x => new AppleMobileListItemResponse(x))
            .ToList();

        if (listData is not null)
        {
            foreach (var item in listData)
            {
                item.Url = UnfurlFastGooeyLink(item.Url, gooeyInterface.Workspace.PublicId);
            }
        }

        return new AppleMobileListHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Title = gooeyInterface.Name,
            Content = listData
        };
    }

    private AppleMobileContentHypermediaResponse GenerateAppleMobileContent(GooeyInterface gooeyInterface)
    {
        var options = new JsonSerializerOptions
        {
            // This tells the serializer to look ahead/buffer properties if $type isn't first
            AllowOutOfOrderMetadataProperties = true,
            // Ensure case insensitivity matches your likely needs
            PropertyNameCaseInsensitive = true
        };

        var content = gooeyInterface.Config.Deserialize<AppleMobileContentJsonDataModel>(options);
        var viewContent = JsonSerializer.SerializeToNode(content?.Items ?? []) as JsonArray;
        UnfurlFastGooeyLinksAndReturn(viewContent, gooeyInterface.Workspace.PublicId);

        return new AppleMobileContentHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Title = gooeyInterface.Name,
            Content = viewContent
        };
    }

    private AppleMobileCollectionHypermediaResponse GenerateAppleMobileCollection(GooeyInterface gooeyInterface)
    {
        var content = gooeyInterface.Config.Deserialize<AppleMobileCollectionViewJsonDataModel>();
        var collectionData = content?.Items
            .Select(x => new AppleMobileCollectionItemResponse(x))
            .ToList() ?? [];

        foreach (var item in collectionData)
        {
            item.ImageUrl = UnfurlFastGooeyLink(item.ImageUrl, gooeyInterface.Workspace.PublicId);
            item.Url = UnfurlFastGooeyLink(item.Url, gooeyInterface.Workspace.PublicId);
        }

        return new AppleMobileCollectionHypermediaResponse
        {
            Title = gooeyInterface.Name,
            InterfaceId = gooeyInterface.DocId,
            Content = collectionData
        };
    }

    private MacTableHypermediaResponse GenerateMacTable(GooeyInterface gooeyInterface)
    {
        var content = gooeyInterface.Config.Deserialize<MacTableJsonDataModel>();
        var headers = content?.Header
            .Select(x => new MacTableHeaderResponse
            {
                Alias = x.FieldAlias,
                Title = x.FieldName
            }).ToList();

        var tableData = new JsonArray();

        if (content?.Data is not null)
        {
            foreach (var item in content.Data)
            {
                var rowJson = new JsonObject();

                // It's usually helpful to include the row identifier for the UI to track selection
                rowJson.Add("id", item.Identifier);
                rowJson.Add("gooeyName", item.GooeyName);
                rowJson.Add("relatedUrl", item.RelatedUrl);
                rowJson.Add("doubleClickUrl", item.DoubleClickUrl);

                // Only add fields that are defined in the Header configuration
                foreach (var colDef in content.Header)
                {
                    // Look up data using the Internal Name (FieldName)
                    if (item.Content.TryGetValue(colDef.FieldAlias, out var value))
                    {
                        // Map it to the External Name (FieldAlias)
                        // If the source is already a JsonNode, clone it so this row owns its own node tree.
                        JsonNode? nodeValue = value switch
                        {
                            JsonNode jsonNode => jsonNode.DeepClone(),
                            _ => JsonSerializer.SerializeToNode(value)
                        };

                        rowJson.Add(colDef.FieldAlias, nodeValue);
                    }
                    else
                    {
                        // Ensure the key exists even if data is missing
                        rowJson.Add(colDef.FieldAlias, null);
                    }
                }

                UnfurlFastGooeyLinksAndReturn(rowJson, gooeyInterface.Workspace.PublicId);
                tableData.Add(rowJson);
            }
        }

        return new MacTableHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Content = new MacTableContent
            {
                Headers = headers,
                TableContent = tableData
            }
        };
    }

    private MacSourceListHypermediaResponse GenerateMacSourceList(GooeyInterface gooeyInterface)
    {
        var content = gooeyInterface.Config.Deserialize<MacSourceListJsonDataModel>();
        var sourceListGroups = content?.Groups
            .Select(x => new MacSourceListGroupResponse(x)).ToList();

        if (sourceListGroups is not null)
        {
            foreach (var group in sourceListGroups)
            {
                foreach (var item in group.GroupItems)
                {
                    item.Url = UnfurlFastGooeyLink(item.Url, gooeyInterface.Workspace.PublicId);
                }
            }
        }

        return new MacSourceListHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Content = new MacSourceListContent
            {
                Groups = sourceListGroups ?? []
            }
        };
    }

    private MacCollectionHypermediaResponse GenerateMacCollection(GooeyInterface gooeyInterface)
    {
        var content = gooeyInterface.Config.Deserialize<MacCollectionViewJsonDataModel>();
        var collectionData = content?.Items
            .Select(x => new MacCollectionItemResponse(x))
            .ToList() ?? [];

        foreach (var item in collectionData)
        {
            item.ImageUrl = UnfurlFastGooeyLink(item.ImageUrl, gooeyInterface.Workspace.PublicId);
            item.Url = UnfurlFastGooeyLink(item.Url, gooeyInterface.Workspace.PublicId);
        }

        return new MacCollectionHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Content = collectionData
        };
    }

    private MacContentHypermediaResponse GenerateMacContent(GooeyInterface gooeyInterface)
    {
        var options = new JsonSerializerOptions
        {
            // This tells the serializer to look ahead/buffer properties if $type isn't first
            AllowOutOfOrderMetadataProperties = true,
            // Ensure case insensitivity matches your likely needs
            PropertyNameCaseInsensitive = true
        };

        var content = gooeyInterface.Config.Deserialize<MacContentJsonDataModel>(options);
        var viewContent = JsonSerializer.SerializeToNode(content?.Items ?? []) as JsonArray;
        UnfurlFastGooeyLinksAndReturn(viewContent, gooeyInterface.Workspace.PublicId);

        return new MacContentHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Content = viewContent
        };
    }

    private MacOutlineHypermediaResponse GenerateMacOutline(GooeyInterface gooeyInterface)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        List<MacOutlineJsonDataModel> rootItems = [];

        // Handle both Array (Forest) and Object (Single Root) inputs from the DB
        if (gooeyInterface.Config.RootElement.ValueKind is JsonValueKind.Array)
        {
            rootItems = gooeyInterface.Config.Deserialize<List<MacOutlineJsonDataModel>>(options) ?? [];
        }
        else if (gooeyInterface.Config.RootElement.ValueKind is JsonValueKind.Object)
        {
            var singleRoot = gooeyInterface.Config.Deserialize<MacOutlineJsonDataModel>(options);
            if (singleRoot is not null) rootItems.Add(singleRoot);
        }

        if (rootItems.Count == 1 && string.Equals(rootItems[0].Name, "Root", StringComparison.OrdinalIgnoreCase))
        {
            rootItems = rootItems[0].Children ?? [];
        }

        // Start recursion at Depth 1, limit to 12
        var responseItems = rootItems
            .Select(x => new MacOutlineItemResponse(x, currentDepth: 1, maxDepth: 12))
            .ToList();
        UnfurlFastGooeyLinks(responseItems, gooeyInterface.Workspace.PublicId);

        return new MacOutlineHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Content = responseItems
        };
    }

    private string UnfurlFastGooeyLink(string? value, Guid workspaceId)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
        }

        if (value.StartsWith(FastGooeyMediaScheme, StringComparison.OrdinalIgnoreCase))
        {
            return UnfurlFastGooeyPublicMediaLink(value, workspaceId);
        }

        if (!value.StartsWith(FastGooeyLinkScheme, StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        var idValue = value.Substring(FastGooeyLinkScheme.Length);
        if (!Guid.TryParse(idValue, out var interfaceId))
        {
            return value;
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return $"{baseUrl}/hypermedia/{interfaceId.ToBase64Url()}";
    }

    private string UnfurlFastGooeyPublicMediaLink(string value, Guid workspaceId)
    {
        if (!TryParseMediaLink(value, out var sourceId, out var path))
        {
            return value;
        }

        var url = Url.Action(
            "Preview",
            "PublicMedia",
            new { workspaceId, sourceId, path },
            Request.Scheme,
            Request.Host.ToString());

        return url ?? value;
    }

    private static bool TryParseMediaLink(string value, out Guid sourceId, out string path)
    {
        sourceId = Guid.Empty;
        path = string.Empty;

        if (!value.StartsWith(FastGooeyMediaScheme, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var remainder = value.Substring(FastGooeyMediaScheme.Length);
        var separatorIndex = remainder.IndexOf(':');
        if (separatorIndex <= 0 || separatorIndex >= remainder.Length - 1)
        {
            return false;
        }

        var sourceValue = remainder[..separatorIndex];
        var pathValue = remainder[(separatorIndex + 1)..];
        if (!Guid.TryParse(sourceValue, out sourceId))
        {
            return false;
        }

        path = Uri.UnescapeDataString(pathValue);
        return true;
    }

    private JsonNode? UnfurlFastGooeyLinksAndReturn(JsonNode? node, Guid workspaceId)
    {
        if (node is null)
        {
            return null;
        }

        if (node is JsonValue valueNode && valueNode.TryGetValue<string>(out var stringValue))
        {
            return JsonValue.Create(UnfurlFastGooeyLink(stringValue, workspaceId));
        }

        if (node is JsonObject obj)
        {
            foreach (var pair in obj.ToList())
            {
                if (pair.Value is JsonValue value && value.TryGetValue<string>(out var jsonObjStringValue))
                {
                    obj[pair.Key] = JsonValue.Create(UnfurlFastGooeyLink(jsonObjStringValue, workspaceId));
                    continue;
                }

                UnfurlFastGooeyLinksAndReturn(pair.Value, workspaceId);
            }

            return obj;
        }

        if (node is JsonArray arr)
        {
            for (var i = 0; i < arr.Count; i++)
            {
                var current = arr[i];
                if (current is JsonValue value && value.TryGetValue<string>(out var jsonArrayStringValue))
                {
                    arr[i] = JsonValue.Create(UnfurlFastGooeyLink(jsonArrayStringValue, workspaceId));
                    continue;
                }

                UnfurlFastGooeyLinksAndReturn(current, workspaceId);
            }

            return arr;
        }

        return node;
    }

    private void UnfurlFastGooeyLinks(IEnumerable<MacOutlineItemResponse> items, Guid workspaceId)
    {
        foreach (var item in items)
        {
            item.Url = UnfurlFastGooeyLink(item.Url, workspaceId);
            if (item.Children.Count > 0)
            {
                UnfurlFastGooeyLinks(item.Children, workspaceId);
            }
        }
    }
}
