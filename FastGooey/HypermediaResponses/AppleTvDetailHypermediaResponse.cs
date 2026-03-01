using FastGooey.Features.Interfaces.AppleTv.Detail.Models;

namespace FastGooey.HypermediaResponses;

public class AppleTvDetailHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "AppleTv";
    public string View { get; set; } = "Detail";
    public string Title { get; set; } = string.Empty;
    
    public AppleTvDetailContent Content { get; set; } = new();
}

public class AppleTvDetailContent
{
    public string Description { get; set; } = string.Empty;
    public string PreviewMediaUrl { get; set; } = string.Empty;
    public List<AppleTvDetailRelatedItemJsonModel> RelatedItems { get; set; } = [];
}
