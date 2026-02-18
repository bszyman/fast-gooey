using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.ViewModels.AppleTv;

public class AppleTvInterfaceMainViewModel
{
    public AppleTvMainWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToBase64Url();
    }
}