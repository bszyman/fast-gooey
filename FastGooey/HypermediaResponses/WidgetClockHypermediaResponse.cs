using FastGooey.Models.JsonDataModels;
using FastGooey.Models.UtilModels;

namespace FastGooey.HypermediaResponses;

public class WidgetClockHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "Widget";
    public string View { get; set; } = "Clock";
    public string Location { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;

    public WidgetClockHypermediaResponse()
    {
    }

    public WidgetClockHypermediaResponse(ClockJsonDataModel model, LocationDateTimeSetModel dateTimeSet)
    {
        Location = model.Location;
        Time = dateTimeSet.LocalTime;
        Date = dateTimeSet.LocalDate;
        Timezone = dateTimeSet.LocalTimezone;
    }
}
