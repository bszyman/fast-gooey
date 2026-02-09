using System.Text.Json;
using System.Text.Json.Nodes;
using FastGooey.Database;
using FastGooey.HypermediaResponses;
using FastGooey.Models;
using FastGooey.Models.JsonDataModels;
using FastGooey.Models.JsonDataModels.Mac;
using FastGooey.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers;

[Route("hypermedia")]
public class HypermediaController(ApplicationDbContext dbContext) : Controller
{
    private const string FastGooeyLinkScheme = "fastgooey:";

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Get(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
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

        return hypermediaResponse != null ? Ok(hypermediaResponse) : NotFound();
    }

    private IHypermediaResponse GenerateAppleMobileResponse(GooeyInterface gooeyInterface)
    {
        switch (gooeyInterface.ViewType)
        {
            case "List":
                return GenerateAppleMobileList(gooeyInterface);
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

    private NotSupported NotSupported()
    {
        return new NotSupported();
    }

    private AppleMobileListHypermediaResponse GenerateAppleMobileList(GooeyInterface gooeyInterface)
    {
        var content = gooeyInterface.Config.Deserialize<AppleMobileListJsonDataModel>();
        var listData = content?.Items
            .Select(x => new AppleMobileListItemResponse(x))
            .ToList();

        if (listData != null)
        {
            foreach (var item in listData)
            {
                item.Url = UnfurlFastGooeyLink(item.Url);
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
        UnfurlFastGooeyLinksAndReturn(viewContent);

        return new AppleMobileContentHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Title = gooeyInterface.Name,
            Content = viewContent
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
                        // SerializeToNode converts the 'object' (which is likely a JsonElement) into a node compatible with JsonObject
                        rowJson.Add(colDef.FieldAlias, JsonSerializer.SerializeToNode(value));
                    }
                    else
                    {
                        // Ensure the key exists even if data is missing
                        rowJson.Add(colDef.FieldAlias, null);
                    }
                }

                UnfurlFastGooeyLinksAndReturn(rowJson);
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

        if (sourceListGroups != null)
        {
            foreach (var group in sourceListGroups)
            {
                foreach (var item in group.GroupItems)
                {
                    item.Url = UnfurlFastGooeyLink(item.Url);
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
        UnfurlFastGooeyLinksAndReturn(viewContent);

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
        UnfurlFastGooeyLinks(responseItems);

        return new MacOutlineHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Content = responseItems
        };
    }

    private string UnfurlFastGooeyLink(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
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

    private JsonNode? UnfurlFastGooeyLinksAndReturn(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        if (node is JsonValue valueNode && valueNode.TryGetValue<string>(out var stringValue))
        {
            return JsonValue.Create(UnfurlFastGooeyLink(stringValue));
        }

        if (node is JsonObject obj)
        {
            foreach (var pair in obj.ToList())
            {
                obj[pair.Key] = UnfurlFastGooeyLinksAndReturn(pair.Value);
            }

            return obj;
        }

        if (node is JsonArray arr)
        {
            for (var i = 0; i < arr.Count; i++)
            {
                arr[i] = UnfurlFastGooeyLinksAndReturn(arr[i]);
            }

            return arr;
        }

        return node;
    }

    private void UnfurlFastGooeyLinks(IEnumerable<MacOutlineItemResponse> items)
    {
        foreach (var item in items)
        {
            item.Url = UnfurlFastGooeyLink(item.Url);
            if (item.Children.Count > 0)
            {
                UnfurlFastGooeyLinks(item.Children);
            }
        }
    }
}
