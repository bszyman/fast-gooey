using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv.Accessories;

namespace FastGooey.HypermediaResponses;

public class AppleTvMainHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "AppleTv";
    public string View { get; set; } = "Main";
    public BackgroundSplash BackgroundSplash { get; set; } = new();
    public List<NavigationButtonJsonDataModel> MenuBarButtons { get; set; } = [];
}
