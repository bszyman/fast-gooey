using FastGooey.Features.Interfaces.AppleTv.Detail.Models;

namespace FastGooey.HypermediaResponses;

public class AppleTvProductHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "AppleTv";
    public string View { get; set; } = "Product";
    public string Title { get; set; } = string.Empty;
    
    public AppleTvProductContent Content { get; set; } = new();
}

public class AppleTvProductContent
{
    public string Description { get; set; } = string.Empty;
    public string PreviewMediaUrl { get; set; } = string.Empty;
    public List<AppleTvDetailRelatedItemJsonModel> RelatedItems { get; set; } = [];
}
