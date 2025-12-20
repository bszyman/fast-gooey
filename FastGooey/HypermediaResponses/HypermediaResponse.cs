namespace FastGooey.HypermediaResponses;

public interface IHypermediaResponse
{
    public Guid InterfaceId { get; set; }
    public string Platform { get; set; }
    public string View { get; set; }
}