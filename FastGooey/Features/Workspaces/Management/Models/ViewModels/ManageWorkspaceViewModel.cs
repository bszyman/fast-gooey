using FastGooey.Features.Media.Shared.Models.ViewModels.Media;
using FastGooey.Features.Workspaces.Management.Models.FormModels;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.ViewModels;

namespace FastGooey.Features.Workspaces.Management.Models.ViewModels;

public class ManageWorkspaceViewModel
{
    public Workspace? Workspace { get; set; }
    public WorkspaceManagementModel? FormModel { get; set; }
    public IReadOnlyList<MediaSourceSummaryViewModel> MediaSources { get; set; } = [];
    public MediaSourceEditorViewModel MediaSourceEditor { get; set; } = new();
    public IReadOnlyList<WorkspaceUserViewModel> WorkspaceUsers { get; set; } = [];
    public WorkspaceUserInviteFormModel InviteUserFormModel { get; set; } = new();
    public bool CanManageWorkspaceUsers { get; set; }
    public string StandardCheckoutUrl { get; set; } = string.Empty;
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}
