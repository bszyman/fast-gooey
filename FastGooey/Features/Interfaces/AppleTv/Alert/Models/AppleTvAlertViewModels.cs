using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleTv.Alert.Models;

public class AppleTvAlertIndexViewModel
{
    public AppleTvAlertWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToBase64Url();
    }
}

public class AppleTvAlertLowerButtonEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string LowerButtonText { get; set; } = string.Empty;
    public string LowerButtonLink { get; set; } = string.Empty;
}

public class AppleTvAlertUpperButtonEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; } = Guid.Empty;
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string UpperButtonText { get; set; } = string.Empty;
    public string UpperButtonLink { get; set; } = string.Empty;
}

public class AppleTvAlertWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public AlertContentJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}
