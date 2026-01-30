using System.Text.Json.Nodes;
using FastGooey.Models.JsonDataModels;

namespace FastGooey.HypermediaResponses;

public class AppleMobileListHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string Platform { get; set; } = "AppleMobile";
    public string View { get; set; } = "List";

    public List<AppleMobileListItemResponse> Content { get; set; } = new();
}

public class AppleMobileListItemResponse
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Guid Identifier { get; set; } = Guid.Empty;

    public AppleMobileListItemResponse() { }

    public AppleMobileListItemResponse(AppleMobileListItemJsonDataModel content)
    {
        Title = content.Title;
        Subtitle = content.Subtitle;
        Url = content.Url;
        Identifier = content.Identifier;
    }
}