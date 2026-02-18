using FastGooey.Models.JsonDataModels;

namespace FastGooey.Features.Interfaces.AppleMobile.Shared.Models.ViewModels.AppleMobileInterface;

public class AppleMobileVideoConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public VideoContentItem? Content { get; set; }
}