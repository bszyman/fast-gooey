using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class ProductData
{
    /// <summary>
    /// Descriptive information about the weather data.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; set; }

}