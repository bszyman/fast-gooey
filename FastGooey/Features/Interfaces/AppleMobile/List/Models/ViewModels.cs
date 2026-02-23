using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleMobile.List.Models;

public class AppleMobileInterfaceListEditorViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public AppleMobileListItemJsonDataModel? Item { get; set; }
}

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

public class AppleMobileInterfaceListWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public AppleMobileListJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}