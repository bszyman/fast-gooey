using System.ComponentModel.DataAnnotations;
using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.Mac.SourceList.Models;

public class MacInterfaceSourceListGroupEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; }
    public Guid InterfaceId { get; set; }
    public Guid GroupId { get; set; } = Guid.Empty;

    [Required]
    public string GroupName { get; set; } = string.Empty;
}

public class MacInterfaceSourceListGroupItemEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; }
    public Guid InterfaceId { get; set; }
    public Guid GroupId { get; set; } = Guid.Empty;

    public Guid Identifier { get; set; } = Guid.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    [Required]
    public string Url { get; set; } = string.Empty;
}

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

public class MacInterfaceSourceListWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacSourceListJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}