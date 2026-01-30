using System.Text.Json;
using System.Text.Json.Nodes;
using FastGooey.Database;
using FastGooey.HypermediaResponses;
using FastGooey.Models;
using FastGooey.Models.JsonDataModels;
using FastGooey.Models.JsonDataModels.Mac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers;

[Route("hypermedia")]
public class HypermediaController(ApplicationDbContext dbContext) : Controller
{
    [HttpGet("{interfaceId:guid}")]
    public async Task<IActionResult> Get(Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstOrDefaultAsync(x => x.DocId.Equals(interfaceId));

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
            if (singleRoot is not null)
                rootItems.Add(singleRoot);
        }

        // Start recursion at Depth 1, limit to 12
        var responseItems = rootItems
            .Select(x => new MacOutlineItemResponse(x, currentDepth: 1, maxDepth: 12))
            .ToList();

        return new MacOutlineHypermediaResponse
        {
            InterfaceId = gooeyInterface.DocId,
            Content = responseItems
        };
    }
}