using FastGooey.Models.ViewModels.Map;

namespace FastGooey.Models.ViewModels.Clock;

public class ClockSearchPanelViewModel
{
    public string? SearchText { get; set; }
    public IEnumerable<MapKitSearchResponseModelWithTime>? Results { get; set; }
}