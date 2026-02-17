namespace FastGooey.Models.ViewModels.AppleTv;

public class AppleTvMainMenuBarEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public List<AppleTvMainMenuBarButtonViewModel> Items { get; set; } = [];
}

public class AppleTvMainMenuBarButtonViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}
