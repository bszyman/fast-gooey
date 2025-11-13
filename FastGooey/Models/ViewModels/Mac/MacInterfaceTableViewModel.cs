namespace FastGooey.Models.ViewModels.Mac;

public class MacInterfaceTableViewModel
{
    public MacInterfaceTableWorkspaceViewModel? Workspace { get; set; }
    
    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToString();
    }
}