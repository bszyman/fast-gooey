using MapKit.Models;

namespace FastGooey.Features.Widgets.Map.Models.ViewModels.Map;

public class MapSearchPanelViewModel
{
    public string? SearchText { get; set; }
    public MapKitSearchResponseModel? Results { get; set; }
    public Guid WorkspaceId { get; set; }
}