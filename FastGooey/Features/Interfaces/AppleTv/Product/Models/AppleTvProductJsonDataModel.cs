using System.Text.Json;

namespace FastGooey.Features.Interfaces.AppleTv.Product.Models;

public class AppleTvProductJsonDataModel
{
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    public string PreviewMediaUrl { get; set; } = string.Empty;
    
    public List<AppleTvProductRelatedItemJsonModel> RelatedProducts { get; set; } = [];
}

public class AppleTvProductRelatedItemJsonModel
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;

    public AppleTvProductRelatedItemJsonModel()
    {
    }

    public AppleTvProductRelatedItemJsonModel(JsonElement element)
    {
        Id = TryReadGuid(element, "Id", "id");
        Title = ReadString(element, "Title", "title");
        Link = ReadString(element, "Link", "link");
        MediaUrl = ReadString(element, "MediaUrl", "mediaUrl");
    }

    private static string ReadString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static Guid TryReadGuid(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value) &&
                value.ValueKind == JsonValueKind.String &&
                Guid.TryParse(value.GetString(), out var id))
            {
                return id;
            }
        }

        return Guid.Empty;
    }
}
