using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.Mac.Outline.Models;

public class MacOutlineEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; }
    public Guid InterfaceId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string ParentId { get; set; } = string.Empty;
    public Guid Identifier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class MacOutlineViewModel
{
    public MacOutlineWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToBase64Url();
    }
}

public class MacOutlineWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacOutlineJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}