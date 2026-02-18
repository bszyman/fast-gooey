using FastGooey.Features.Widgets.Weather.Models.JsonDataModels;
using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Widgets.Weather.Models.ViewModels.Weather;

public class WeatherWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public WeatherJsonDataModel? Data { get; set; }

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}