using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class WeatherAlertCollection
{
    /// <summary>
    /// An array of weather alert summaries.
    /// </summary>
    [JsonPropertyName("alerts")]
    public WeatherAlertSummary[] Alerts { get; set; }

    /// <summary>
    /// A URL that provides more information about the alerts.
    /// </summary>
    [JsonPropertyName("detailsUrl")]
    public string DetailsUrl { get; set; }

}