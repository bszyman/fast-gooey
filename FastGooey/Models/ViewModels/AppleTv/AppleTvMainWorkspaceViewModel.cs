using FastGooey.Models.JsonDataModels.AppleTv;
using FastGooey.Utils;

namespace FastGooey.Models.ViewModels.AppleTv;

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
