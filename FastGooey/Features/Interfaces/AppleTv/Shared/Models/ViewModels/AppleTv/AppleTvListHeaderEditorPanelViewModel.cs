namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.ViewModels.AppleTv;

public class AppleTvListHeaderEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
