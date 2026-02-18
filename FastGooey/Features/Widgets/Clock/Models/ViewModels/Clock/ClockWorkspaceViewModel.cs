using FastGooey.Features.Widgets.Clock.Models.JsonDataModels;
using FastGooey.Models;
using FastGooey.Utils;
using FastGooey.Models.UtilModels;

namespace FastGooey.Features.Widgets.Clock.Models.ViewModels.Clock;

public class ClockWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public ClockJsonDataModel? Data { get; set; }
    public LocationDateTimeSetModel? CurrentTime { get; set; }

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}