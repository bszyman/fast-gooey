
using System.Text.Json.Nodes;

namespace FastGooey.HypermediaResponses;

public class MacContentHypermediaResponse: HypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "Mac";
    public string View { get; set; } = "Content";
    
    public JsonArray Content { get; set; } = new();
}

// public class MacContent
// {
//     public JsonArray ViewContent { get; set; } = [];
// }