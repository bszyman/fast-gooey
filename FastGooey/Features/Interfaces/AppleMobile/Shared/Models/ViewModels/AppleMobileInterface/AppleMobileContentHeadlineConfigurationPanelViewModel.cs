using FastGooey.Models.JsonDataModels;

namespace FastGooey.Features.Interfaces.AppleMobile.Shared.Models.ViewModels.AppleMobileInterface;

public class AppleMobileContentHeadlineConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public HeadlineContentItem? Content { get; set; }
}