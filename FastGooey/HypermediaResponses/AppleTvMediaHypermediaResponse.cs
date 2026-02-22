namespace FastGooey.HypermediaResponses;

public class AppleTvMediaHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "AppleTv";
    public string View { get; set; } = "Media";
    public string MediaUrl { get; set; } = string.Empty;
}
