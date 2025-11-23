using System.Text.Json.Nodes;

namespace FastGooey.HypermediaResponses;

public class NotSupported: HypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "Any";
    public string View { get; set; } = "Not-Supported";
}