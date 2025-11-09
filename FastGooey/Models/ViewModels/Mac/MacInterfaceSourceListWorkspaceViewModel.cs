using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.Mac;

public class MacInterfaceSourceListWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacSourceListJsonDataModel Data { get; set; } = new();
}