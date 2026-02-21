namespace FastGooey.Features.Interfaces.AppleTv.DescriptiveAlert.Models;

public class AppleTvDescriptiveAlertIndexViewModel
{
    public DescriptiveAlertWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.InterfaceId();
    }
}
