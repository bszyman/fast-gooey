using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleTv.MediaGrid.Models;

public class MediaWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public AppleTvMediaGridJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}

public class MediaIndexViewModel
{
    public MediaWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToBase64Url();
    }
}

public class MediaGridItemPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? LinkTo { get; set; }
    public string PreviewMedia { get; set; } = string.Empty;
}
