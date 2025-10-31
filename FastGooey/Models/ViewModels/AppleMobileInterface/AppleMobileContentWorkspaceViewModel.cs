using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.AppleMobileInterface;

public class AppleMobileContentWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public AppleMobileContentJsonDataModel Data { get; set; } = new();
}