using FastGooey.Models.JsonDataModels;

namespace FastGooey.HypermediaResponses;

public class WidgetWeatherHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "Widget";
    public string View { get; set; } = "Weather";
    public string Location { get; set; } = string.Empty;
    public bool PreviewAvailable { get; set; }
    public string Temperature { get; set; } = "-";
    public string ConditionCode { get; set; } = string.Empty;

    public WidgetWeatherHypermediaResponse()
    {
    }

    public WidgetWeatherHypermediaResponse(WeatherJsonDataModel model)
    {
        Location = model.Location;
    }
}
