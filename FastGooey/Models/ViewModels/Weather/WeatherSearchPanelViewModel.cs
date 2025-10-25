using MapKit.Models;

namespace FastGooey.Models.ViewModels.Weather;

public class WeatherSearchPanelViewModel
{
    public string? SearchText { get; set; }
    public MapKitSearchResponseModel? Results { get; set; }
}