using FastGooey.Models.JsonDataModels.Mac;

namespace FastGooey.Models.ViewModels.Mac;

public class MacInterfaceTableWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacTableJsonDataModel Data { get; set; } = new();
}