using FastGooey.Models.Common;
using FastGooey.Utils;

namespace FastGooey.Models.ViewModels.Mac;

public class MacContentViewModel : ContentViewModelBase<MacContentWorkspaceViewModel>
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