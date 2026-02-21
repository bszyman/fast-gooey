using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleTv.Alert.Models;

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
