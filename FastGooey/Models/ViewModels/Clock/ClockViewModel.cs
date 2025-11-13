namespace FastGooey.Models.ViewModels.Clock;

public class ClockViewModel
{
    public ClockWorkspaceViewModel? WorkspaceViewModel { get; set; }
    
    public string WorkspaceId()
    {
        return WorkspaceViewModel!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return WorkspaceViewModel!.ContentNode!.DocId.ToString();
    }
}