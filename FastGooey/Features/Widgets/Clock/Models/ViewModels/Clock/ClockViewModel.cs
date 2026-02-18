using FastGooey.Utils;
namespace FastGooey.Features.Widgets.Clock.Models.ViewModels.Clock;

public class ClockViewModel
{
    public ClockWorkspaceViewModel? WorkspaceViewModel { get; set; }

    public string WorkspaceId()
    {
        return WorkspaceViewModel!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return WorkspaceViewModel!.ContentNode!.DocId.ToBase64Url();
    }
}