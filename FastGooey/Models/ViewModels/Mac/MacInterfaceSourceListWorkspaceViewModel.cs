using FastGooey.Models.JsonDataModels.Mac;

namespace FastGooey.Models.ViewModels.Mac;

public class MacInterfaceSourceListWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacSourceListJsonDataModel Data { get; set; } = new();
}