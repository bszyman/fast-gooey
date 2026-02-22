namespace FastGooey.Features.Interfaces.AppleTv.Media.Models;

public class AppleTvMediaIndexViewModel
{
    public MediaWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.InterfaceId();
    }
}
