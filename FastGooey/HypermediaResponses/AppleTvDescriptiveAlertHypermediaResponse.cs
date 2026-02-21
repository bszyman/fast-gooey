using FastGooey.Features.Interfaces.AppleTv.DescriptiveAlert.Models;

namespace FastGooey.HypermediaResponses;

public class AppleTvDescriptiveAlertHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "AppleTv";
    public string View { get; set; } = "DescriptiveAlert";
    public string Title { get; set; } = string.Empty;
    public string CancelButtonText { get; set; } = string.Empty;
    public string ConfirmButtonText { get; set; } = string.Empty;
    public List<DescriptiveAlertContentNodeJsonDataModel> DescriptiveContent { get; set; } = [];
}
