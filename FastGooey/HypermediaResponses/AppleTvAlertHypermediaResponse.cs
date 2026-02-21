namespace FastGooey.HypermediaResponses;

public class AppleTvAlertHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "AppleTv";
    public string View { get; set; } = "Alert";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UpperButtonText { get; set; } = string.Empty;
    public string UpperButtonLink { get; set; } = string.Empty;
    public string LowerButtonText { get; set; } = string.Empty;
    public string LowerButtonLink { get; set; } = string.Empty;
}
