using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.RssFeed;

public class RssWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public RssFeedJsonDataModel? Data { get; set; }
}