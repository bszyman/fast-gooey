using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class HourlyForecast
{
    /// <summary>
    /// An array of hourly forecasts.
    /// </summary>
    [JsonPropertyName("hours")]
    public List<HourWeatherConditions> Hours { get; set; }

}