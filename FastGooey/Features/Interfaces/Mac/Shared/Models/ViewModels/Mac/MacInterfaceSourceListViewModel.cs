using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.Mac.Shared.Models.ViewModels.Mac;

public class MacInterfaceSourceListViewModel
{
    public MacInterfaceSourceListWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToBase64Url();
    }
}