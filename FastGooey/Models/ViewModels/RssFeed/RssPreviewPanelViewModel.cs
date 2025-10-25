using FastGooey.Models.Response;

namespace FastGooey.Models.ViewModels.RssFeed;

public class RssPreviewPanelViewModel
{
    public string? FeedTitle { get; set; }
    public string? FeedDescription { get; set; }
    public string? FeedUrl { get; set; }
    public List<RssFeedItem> Items { get; set; } = new();
}