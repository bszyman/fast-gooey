using FastGooey.Features.Interfaces.Mac.Outline.Models;

namespace FastGooey.HypermediaResponses;

public class MacOutlineHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "Mac";
    public string View { get; set; } = "Outline";
    public List<MacOutlineItemResponse> Content { get; set; } = new();
}

public class MacOutlineContent
{
    public List<MacOutlineItemResponse> Items { get; set; } = [];
}

public class MacOutlineItemResponse
{
    public Guid Identifier { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<MacOutlineItemResponse> Children { get; set; } = [];

    public MacOutlineItemResponse() { }

    public MacOutlineItemResponse(MacOutlineJsonDataModel model, int currentDepth, int maxDepth)
    {
        Identifier = model.Identifier;
        Name = model.Name;
        Url = model.Url;

        // Only map children if we haven't hit the limit
        if (currentDepth < maxDepth)
        {
            Children = model.Children?
                .Select(c => new MacOutlineItemResponse(c, currentDepth + 1, maxDepth))
                .ToList() ?? [];
        }
    }
}