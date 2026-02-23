using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv;
using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleTv.Main.Models;

public class AppleTvInterfaceMainViewModel
{
    public AppleTvMainWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToBase64Url();
    }
}

public class AppleTvMainBackgroundEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string ImageResource { get; set; } = string.Empty;
    public string AudioResource { get; set; } = string.Empty;
}

public class AppleTvMainMenuBarEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public List<AppleTvMainMenuBarButtonViewModel> Items { get; set; } = [];
}

public class AppleTvMainMenuBarButtonViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}

public class AppleTvMainWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MainJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}