using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleTv.Product.Models;

public class AppleTvInterfaceProductViewModel
{
    public AppleTvProductWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToBase64Url();
    }
}

public class AppleTvProductWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public AppleTvProductJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}

public class RelatedItemPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public Guid RelatedItemId { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
}