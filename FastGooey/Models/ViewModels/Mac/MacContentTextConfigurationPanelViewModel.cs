using FastGooey.Models.JsonDataModels.Mac;

namespace FastGooey.Models.ViewModels.Mac;

public class MacContentTextConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public TextContentItem? Content { get; set; }
}