using FastGooey.Models.JsonDataModels.Mac;

namespace FastGooey.Models.ViewModels.Mac;

public class MacContentHeadlineConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public HeadlineContentItem? Content { get; set; }
}