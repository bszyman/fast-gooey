using FastGooey.Models.JsonDataModels;
using FastGooey.Models.UtilModels;

namespace FastGooey.Models.ViewModels.Clock;

public class ClockWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public ClockJsonDataModel? Data { get; set; }
    public LocationDateTimeSetModel? CurrentTime { get; set; }
}