using FastGooey.Models.JsonDataModels.Mac;

namespace FastGooey.Models.ViewModels.Mac;

public class MacContentVideoConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public VideoContentItem? Content { get; set; }
}