namespace FastGooey.HypermediaResponses;

public interface HypermediaResponse
{
    public Guid InterfaceId { get; set; }
    public string Platform { get; set; }
    public string View { get; set; }
}