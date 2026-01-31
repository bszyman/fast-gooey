using FastGooey.Utils;
using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.AppleMobileInterface;

public class AppleMobileInterfaceListWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public AppleMobileListJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}