using FastGooey.Models.UtilModels;
using MapKit.Models;

namespace FastGooey.Models.ViewModels;

public class MapKitSearchResponseModelWithTime
{
    public MapKitSearchResultsResponseModel? Result { get; set; }
    public LocationDateTimeSetModel? LocalDateTimeSet { get; set; }
}