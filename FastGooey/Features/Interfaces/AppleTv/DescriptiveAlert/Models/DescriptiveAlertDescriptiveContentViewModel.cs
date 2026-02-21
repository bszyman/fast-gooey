namespace FastGooey.Features.Interfaces.AppleTv.DescriptiveAlert.Models;

public class DescriptiveAlertDescriptiveContentViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public List<DescriptiveAlertDescriptiveContentNodeFormModel> DescriptiveContent { get; set; } = [];
}
