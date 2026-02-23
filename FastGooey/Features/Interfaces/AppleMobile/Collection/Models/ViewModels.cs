using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleMobile.Collection.Models;

public class AppleMobileInterfaceCollectionEditorViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public AppleMobileCollectionViewItemJsonDataModel? Item { get; set; }
}

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

public class AppleMobileInterfaceCollectionWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public AppleMobileCollectionViewJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}
