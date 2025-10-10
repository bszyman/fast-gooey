using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class Metadata
{
    /// <summary>
    /// The URL of the legal attribution for the data source.
    /// </summary>
    [JsonPropertyName("attributionURL")]
    public string AttributionURL { get; set; }

    /// <summary>
    /// The time when the weather data is no longer valid.
    /// </summary>
    [JsonPropertyName("expireTime")]
    public DateTime ExpireTime { get; set; }

    /// <summary>
    /// The ISO language code for localizable fields.
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; set; }

    /// <summary>
    /// The latitude of the relevant location.
    /// </summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    /// The longitude of the relevant location.
    /// </summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// The URL of a logo for the data provider.
    /// </summary>
    [JsonPropertyName("providerLogo")]
    public string ProviderLogo { get; set; }

    /// <summary>
    /// The name of the data provider.
    /// </summary>
    [JsonPropertyName("providerName")]
    public string ProviderName { get; set; }

    /// <summary>
    /// The time the weather data was procured.
    /// </summary>
    [JsonPropertyName("readTime")]
    public DateTime ReadTime { get; set; }

    /// <summary>
    /// The time the provider reported the weather data.
    /// </summary>
    [JsonPropertyName("reportedTime")]
    public DateTime? ReportedTime { get; set; }

    /// <summary>
    /// The weather data is temporarily unavailable from the provider.
    /// </summary>
    [JsonPropertyName("temporarilyUnavailable")]
    public bool? TemporarilyUnavailable { get; set; }

    /// <summary>
    /// The system of units that the weather data is reported in. This is set to metric.
    /// </summary>
    [JsonPropertyName("units")]
    public UnitsSystem? Units { get; set; }

    /// <summary>
    /// The data format version.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }

}