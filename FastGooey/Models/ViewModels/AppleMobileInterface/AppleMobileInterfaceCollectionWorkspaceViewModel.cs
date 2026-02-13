using FastGooey.Models.JsonDataModels;
using FastGooey.Utils;

namespace FastGooey.Models.ViewModels.AppleMobileInterface;

public class AppleMobileInterfaceCollectionWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public AppleMobileCollectionViewJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}
