using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.AppleMobileInterface;

public class AppleMobileInterfaceListWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public AppleMobileListJsonDataModel Data { get; set; } = new();
}