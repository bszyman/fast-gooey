using FastGooey.Models.JsonDataModels;

namespace FastGooey.HypermediaResponses;

public class WidgetRssFeedHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "Widget";
    public string View { get; set; } = "RssFeed";
    public string FeedUrl { get; set; } = string.Empty;
    public string FeedTitle { get; set; } = string.Empty;
    public List<WidgetRssFeedItemResponse> Articles { get; set; } = [];

    public WidgetRssFeedHypermediaResponse()
    {
    }

    public WidgetRssFeedHypermediaResponse(RssFeedJsonDataModel model)
    {
        FeedUrl = model.FeedUrl;
    }
}

public class WidgetRssFeedItemResponse
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public DateTime? PublishDate { get; set; }
}
