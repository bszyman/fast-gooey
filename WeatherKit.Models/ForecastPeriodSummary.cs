using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class ForecastPeriodSummary
{
    /// <summary>
    /// The type of precipitation forecasted.
    /// </summary>
    [JsonPropertyName("condition")]
    public PrecipitationType Condition { get; set; }

    /// <summary>
    /// The end time of the forecast.
    /// </summary>
    [JsonPropertyName("endTime")]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// The probability of precipitation during this period.
    /// </summary>
    [JsonPropertyName("precipitationChance")]
    public double PrecipitationChance { get; set; }

    /// <summary>
    /// The precipitation intensity in millimeters per hour.
    /// </summary>
    [JsonPropertyName("precipitationIntensity")]
    public double PrecipitationIntensity { get; set; }

    /// <summary>
    /// The start time of the forecast.
    /// </summary>
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

}