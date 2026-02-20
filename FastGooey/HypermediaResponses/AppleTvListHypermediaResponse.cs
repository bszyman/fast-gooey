using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv;
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv.Accessories;

namespace FastGooey.HypermediaResponses;

public class AppleTvListHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "AppleTv";
    public string View { get; set; } = "List";
    public Banner Banner { get; set; } = new();
    public Header Header { get; set; } = new();
    public List<ListItem> ListItems { get; set; } = [];
}
