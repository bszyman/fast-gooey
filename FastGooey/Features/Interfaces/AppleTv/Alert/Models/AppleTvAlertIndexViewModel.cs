using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleTv.Alert.Models;

public class AppleTvAlertIndexViewModel
{
    public AppleTvAlertWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToBase64Url();
    }
}
