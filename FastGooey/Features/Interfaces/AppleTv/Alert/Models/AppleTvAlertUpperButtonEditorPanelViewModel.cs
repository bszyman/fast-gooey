namespace FastGooey.Features.Interfaces.AppleTv.Alert.Models;

public class AppleTvAlertUpperButtonEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string UpperButtonText { get; set; } = string.Empty;
    public string UpperButtonLink { get; set; } = string.Empty;
}
