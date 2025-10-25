using MapKit.Models;

namespace FastGooey.Models.ViewModels.Map;

public class MapSearchPanelViewModel
{
    public string? SearchText { get; set; }
    public MapKitSearchResponseModel? Results { get; set; }
}