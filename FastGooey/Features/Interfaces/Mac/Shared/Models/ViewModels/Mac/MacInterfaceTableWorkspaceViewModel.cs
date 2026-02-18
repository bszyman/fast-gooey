using FastGooey.Features.Interfaces.Mac.Shared.Models.JsonDataModels.Mac;
using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.Mac.Shared.Models.ViewModels.Mac;

public class MacInterfaceTableWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacTableJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}