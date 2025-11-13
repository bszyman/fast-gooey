namespace FastGooey.Models.ViewModels.Map;

public class MapViewModel
{
    public MapWorkspaceViewModel? WorkspaceViewModel { get; set; }
    
    public string WorkspaceId()
    {
        return WorkspaceViewModel!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return WorkspaceViewModel!.ContentNode!.DocId.ToString();
    }
}