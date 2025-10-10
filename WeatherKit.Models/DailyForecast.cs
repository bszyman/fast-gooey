using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class DailyForecast
{
    /// <summary>
    /// An array of the day forecast weather conditions.
    /// </summary>
    [JsonPropertyName("days")]
    public DayWeatherConditions[] Days { get; set; }

    /// <summary>
    /// A URL that provides more information about the forecast.
    /// </summary>
    [JsonPropertyName("learnMoreURL")]
    public string LearnMoreURL { get; set; }

}