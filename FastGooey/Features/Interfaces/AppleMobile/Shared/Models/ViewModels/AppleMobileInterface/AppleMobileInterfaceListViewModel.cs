using FastGooey.Utils;
namespace FastGooey.Features.Interfaces.AppleMobile.Shared.Models.ViewModels.AppleMobileInterface;

public class AppleMobileInterfaceListViewModel
{
    public AppleMobileInterfaceListWorkspaceViewModel? WorkspaceViewModel { get; set; }

    public string WorkspaceId()
    {
        return WorkspaceViewModel!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return WorkspaceViewModel!.ContentNode!.DocId.ToBase64Url();
    }
}