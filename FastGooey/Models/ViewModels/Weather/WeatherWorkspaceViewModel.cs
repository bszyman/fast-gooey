using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.Weather;

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
        return ContentNode!.DocId.ToString();
    }
}