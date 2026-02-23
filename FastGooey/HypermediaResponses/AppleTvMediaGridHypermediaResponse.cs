using FastGooey.Features.Interfaces.AppleTv.MediaGrid.Models;

namespace FastGooey.HypermediaResponses;

public class AppleTvMediaGridHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "AppleTv";
    public string View { get; set; } = "MediaGrid";
    public string Title { get; set; } = string.Empty;
    public List<AppleTvMediaGridItemJsonDataModel> MediaItems { get; set; } = [];
}
