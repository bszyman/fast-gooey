using FastGooey.Models.JsonDataModels.Mac;

namespace FastGooey.Models.ViewModels.Mac;

public class MacInterfaceTableStructureWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacTableJsonDataModel Data { get; set; } = new();
}