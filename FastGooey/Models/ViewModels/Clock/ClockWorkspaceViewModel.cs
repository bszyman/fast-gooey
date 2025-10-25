using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.Clock;

public class ClockWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public ClockJsonDataModel? Data { get; set; }
}