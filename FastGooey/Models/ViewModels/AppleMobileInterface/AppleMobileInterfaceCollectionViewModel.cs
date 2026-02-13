using FastGooey.Utils;

namespace FastGooey.Models.ViewModels.AppleMobileInterface;

public class AppleMobileInterfaceCollectionViewModel
{
    public AppleMobileInterfaceCollectionWorkspaceViewModel? WorkspaceViewModel { get; set; }

    public string WorkspaceId()
    {
        return WorkspaceViewModel!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return WorkspaceViewModel!.ContentNode!.DocId.ToBase64Url();
    }
}
