using FastGooey.Models.JsonDataModels.Mac;

namespace FastGooey.Models.ViewModels.Mac;

public class MacContentLinkConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public LinkContentItem? Content { get; set; }
}