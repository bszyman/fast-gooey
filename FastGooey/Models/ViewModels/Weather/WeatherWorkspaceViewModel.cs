using FastGooey.Models.JsonDataModels;

namespace FastGooey.Models.ViewModels.Weather;

public class WeatherWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public WeatherJsonDataModel? Data { get; set; }
}