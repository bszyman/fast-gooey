using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleTv.DescriptiveAlert.Models;

public class DescriptiveAlertWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public DescriptiveAlertContentJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}
