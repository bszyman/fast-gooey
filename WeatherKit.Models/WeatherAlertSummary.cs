using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class WeatherAlertSummary
{
    /// <summary>
    /// An official designation of the affected area.
    /// </summary>
    [JsonPropertyName("areaId")]
    public string AreaId { get; set; }

    /// <summary>
    /// A human-readable name of the affected area.
    /// </summary>
    [JsonPropertyName("areaName")]
    public string AreaName { get; set; }

    /// <summary>
    /// How likely the event is to occur.
    /// </summary>
    [JsonPropertyName("certainty")]
    public Certainty Certainty { get; set; }

    /// <summary>
    /// The ISO code of the reporting country.
    /// </summary>
    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; }

    /// <summary>
    /// A human-readable description of the event.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// The URL to a page containing detailed information about the event.
    /// </summary>
    [JsonPropertyName("detailsUrl")]
    public string DetailsUrl { get; set; }

    /// <summary>
    /// The time the event went into effect.
    /// </summary>
    [JsonPropertyName("effectiveTime")]
    public DateTime EffectiveTime { get; set; }

    /// <summary>
    /// The time when the underlying weather event is projected to end.
    /// </summary>
    [JsonPropertyName("eventEndTime")]
    public DateTime? EventEndTime { get; set; }

    /// <summary>
    /// The time when the underlying weather event is projected to start.
    /// </summary>
    [JsonPropertyName("eventOnsetTime")]
    public DateTime? EventOnsetTime { get; set; }

    /// <summary>
    /// The time when the event expires.
    /// </summary>
    [JsonPropertyName("expireTime")]
    public DateTime ExpireTime { get; set; }

    /// <summary>
    /// A unique identifier of the event.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// The time that event was issued by the reporting agency.
    /// </summary>
    [JsonPropertyName("issuedTime")]
    public DateTime IssuedTime { get; set; }

    /// <summary>
    /// An array of recommended actions from the reporting agency.
    /// </summary>
    [JsonPropertyName("responses")]
    public ResponseType[] Responses { get; set; }

    /// <summary>
    /// The level of danger to life and property.
    /// </summary>
    [JsonPropertyName("severity")]
    public Severity Severity { get; set; }

    /// <summary>
    /// The name of the reporting agency.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; }

    /// <summary>
    /// An indication of urgency of action from the reporting agency.
    /// </summary>
    [JsonPropertyName("urgency")]
    public Urgency Urgency { get; set; }

}