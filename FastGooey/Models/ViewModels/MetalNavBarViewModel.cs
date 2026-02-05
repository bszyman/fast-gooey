namespace FastGooey.Models.ViewModels;

public class MetalNavBarViewModel
{
    public string WorkspaceName { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public string ActiveTab { get; set; } = string.Empty;
}