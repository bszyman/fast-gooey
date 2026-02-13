using FastGooey.Models.JsonDataModels.Mac;
using FastGooey.Utils;

namespace FastGooey.Models.ViewModels.Mac;

public class MacInterfaceCollectionWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacCollectionViewJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}
