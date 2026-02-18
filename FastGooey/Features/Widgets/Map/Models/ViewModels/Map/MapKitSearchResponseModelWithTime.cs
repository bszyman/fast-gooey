using FastGooey.Models.UtilModels;
using MapKit.Models;

namespace FastGooey.Features.Widgets.Map.Models.ViewModels.Map;

public class MapKitSearchResponseModelWithTime
{
    public MapKitSearchResultsResponseModel? Result { get; set; }
    public LocationDateTimeSetModel? LocalDateTimeSet { get; set; }
}