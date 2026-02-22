using FastGooey.Features.Interfaces.AppleTv.Product.Models;
using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.ViewModels.AppleTv;

public class AppleTvProductWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public AppleTvProductJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}
