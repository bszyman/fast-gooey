using MapKit.Models;

namespace FastGooey.Models.ViewModels;

public class ClockSearchPanelViewModel
{
    public string? SearchText { get; set; }
    public IEnumerable<MapKitSearchResponseModelWithTime>? Results { get; set; }
}