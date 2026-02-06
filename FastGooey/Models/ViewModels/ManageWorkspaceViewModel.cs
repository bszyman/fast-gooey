using FastGooey.Models.FormModels;

namespace FastGooey.Models.ViewModels;

public class ManageWorkspaceViewModel
{
    public Workspace? Workspace { get; set; }
    public WorkspaceManagementModel? FormModel { get; set; }
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}