using System.Text.Json.Nodes;

namespace FastGooey.HypermediaResponses;

public class MacTableHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "Mac";
    public string View { get; set; } = "Table";

    public MacTableContent Content { get; set; } = new();
}

public class MacTableContent
{
    public List<MacTableHeaderResponse> Headers { get; set; } = [];
    public JsonArray TableContent { get; set; } = [];
}

public class MacTableHeaderResponse
{
    public string Title { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
}