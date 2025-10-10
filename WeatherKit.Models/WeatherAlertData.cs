using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class WeatherAlertData
{
    /// <summary>
    /// The geographic region the weather alert applies to.
    /// </summary>
    [JsonPropertyName("area")]
    public Dictionary<string, string>? Area { get; set; }

    /// <summary>
    /// Official text messages describing a severe weather event.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<EventText>? Messages { get; set; }
}