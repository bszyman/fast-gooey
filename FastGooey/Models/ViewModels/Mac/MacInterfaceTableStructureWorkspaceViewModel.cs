using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.Mac;

public class MacInterfaceTableStructureWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacTableJsonDataModel Data { get; set; } = new();
}