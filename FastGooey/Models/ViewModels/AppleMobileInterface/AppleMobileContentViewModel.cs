using FastGooey.Models.Common;
using FastGooey.Utils;

namespace FastGooey.Models.ViewModels.AppleMobileInterface;

public class AppleMobileContentViewModel : ContentViewModelBase<AppleMobileContentWorkspaceViewModel>
{
    public override string WorkspaceId()
    {
        return WorkspaceViewModel!.ContentNode!.Workspace.PublicId.ToString();
    }

    public override string InterfaceId()
    {
        return WorkspaceViewModel!.ContentNode!.DocId.ToBase64Url();
    }
}