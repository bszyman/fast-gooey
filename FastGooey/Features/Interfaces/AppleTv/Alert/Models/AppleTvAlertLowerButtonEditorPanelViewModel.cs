namespace FastGooey.Features.Interfaces.AppleTv.Alert.Models;

public class AppleTvAlertLowerButtonEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string LowerButtonText { get; set; } = string.Empty;
    public string LowerButtonLink { get; set; } = string.Empty;
}
