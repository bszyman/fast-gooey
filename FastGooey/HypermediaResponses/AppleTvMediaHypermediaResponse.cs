namespace FastGooey.HypermediaResponses;

public class AppleTvMediaHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "AppleTv";
    public string View { get; set; } = "Media";
    public string Title { get; set; } = string.Empty;
    
    public AppleTvMediaContent Content { get; set; } = new();
}

public class AppleTvMediaContent
{
    public string MediaUrl { get; set; } = string.Empty;
}
