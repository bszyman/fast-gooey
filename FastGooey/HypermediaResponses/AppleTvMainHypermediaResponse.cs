using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv.Accessories;

namespace FastGooey.HypermediaResponses;

public class AppleTvMainHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "AppleTv";
    public string View { get; set; } = "Main";
    public string Title { get; set; } = string.Empty;
    
    public AppleTvMainContent Content { get; set; } = new();
}

public class AppleTvMainContent
{
    public BackgroundSplash BackgroundSplash { get; set; } = new();
    public List<NavigationButtonJsonDataModel> MenuBarButtons { get; set; } = [];
}
