using FastGooey.Features.Widgets.Map.Models.JsonDataModels;

namespace FastGooey.HypermediaResponses;

public class WidgetMapHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "Widget";
    public string View { get; set; } = "Map";
    public List<WidgetMapPinResponse> Pins { get; set; } = [];

    public WidgetMapHypermediaResponse()
    {
    }

    public WidgetMapHypermediaResponse(MapJsonDataModel model)
    {
        Pins = model.Pins.Select(x => new WidgetMapPinResponse(x)).ToList();
    }
}

public class WidgetMapPinResponse
{
    public Guid EntryId { get; set; } = Guid.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public string Coordinates { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;

    public WidgetMapPinResponse()
    {
    }

    public WidgetMapPinResponse(MapWorkspacePinModel model)
    {
        EntryId = model.EntryId;
        Latitude = model.Latitude;
        Longitude = model.Longitude;
        Coordinates = model.Coordinates;
        LocationName = model.LocationName;
    }
}
