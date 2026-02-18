using FastGooey.Features.Widgets.Map.Models.ViewModels.Map;

namespace FastGooey.Features.Widgets.Clock.Models.ViewModels.Clock;

public class ClockSearchPanelViewModel
{
    public string? SearchText { get; set; }
    public IEnumerable<MapKitSearchResponseModelWithTime>? Results { get; set; }
}