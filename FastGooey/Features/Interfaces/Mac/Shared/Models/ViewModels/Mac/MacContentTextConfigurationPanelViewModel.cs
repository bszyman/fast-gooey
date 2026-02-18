using FastGooey.Features.Interfaces.Mac.Shared.Models.JsonDataModels.Mac;

namespace FastGooey.Features.Interfaces.Mac.Shared.Models.ViewModels.Mac;

public class MacContentTextConfigurationPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }
    public TextContentItem? Content { get; set; }
}