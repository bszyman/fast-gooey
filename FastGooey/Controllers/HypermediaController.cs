using System.Text.Json;
using System.Text.Json.Nodes;
using FastGooey.Database;
using FastGooey.HypermediaResponses;
using FastGooey.Models;
using FastGooey.Models.JsonDataModels.Mac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers;

[Route("hypermedia")]
public class HypermediaController(ApplicationDbContext dbContext): Controller
{
    [HttpGet("{interfaceId:guid}")]
    public async Task<IActionResult> Get(Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstOrDefaultAsync(x => x.DocId.Equals(interfaceId));

        if (contentNode == null)
        {
            return NotFound();
        }
        
        HypermediaResponse? hypermediaResponse = null;

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

    private HypermediaResponse? GenerateAppleMobileResponse(GooeyInterface gooeyInterface)
    {
        return null;
    }
    
    private HypermediaResponse GenerateMacResponse(GooeyInterface gooeyInterface)
    {
        switch (gooeyInterface.ViewType)
        {
            case "Table":
                return GenerateMacTable(gooeyInterface);
            // case "Source-List":
            //     break;
            // case "Content":
            //     break;
            // case "Outline":
            //     break;
            default:
                return NotSupported();
        }        
    }

    private NotSupported NotSupported()
    {
        return new NotSupported();
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

        if (content?.Data != null)
        {
            foreach (var item in content.Data)
            {
                var rowJson = new JsonObject();
                    
                // It's usually helpful to include the row identifier for the UI to track selection
                rowJson.Add("id", item.Identifier);

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
}