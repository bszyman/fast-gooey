namespace FastGooey.Models.Response;

public class RssFeedItem
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Link { get; set; }
    public DateTime PublishDate { get; set; }
}