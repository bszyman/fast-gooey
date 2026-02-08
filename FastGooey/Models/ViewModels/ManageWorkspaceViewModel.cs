using FastGooey.Models.FormModels;
using FastGooey.Models.ViewModels.Media;

namespace FastGooey.Models.ViewModels;

public class ManageWorkspaceViewModel
{
    public Workspace? Workspace { get; set; }
    public WorkspaceManagementModel? FormModel { get; set; }
    public IReadOnlyList<MediaSourceSummaryViewModel> MediaSources { get; set; } = [];
    public MediaSourceEditorViewModel MediaSourceEditor { get; set; } = new();
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}
