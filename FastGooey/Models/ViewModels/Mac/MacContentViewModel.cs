namespace FastGooey.Models.ViewModels.Mac;

public class MacContentViewModel
{
    public MacContentWorkspaceViewModel? Workspace { get; set; }
    
    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToString();
    }
}