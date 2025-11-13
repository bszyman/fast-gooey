namespace FastGooey.Models.ViewModels.AppleMobileInterface;

public class AppleMobileContentViewModel
{
    public AppleMobileContentWorkspaceViewModel? WorkspaceViewModel { get; set; }
    
    public string WorkspaceId()
    {
        return WorkspaceViewModel!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return WorkspaceViewModel!.ContentNode!.DocId.ToString();
    }
}