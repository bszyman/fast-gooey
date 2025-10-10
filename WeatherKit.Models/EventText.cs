using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class EventText
{
    [JsonPropertyName("language")]
    public string Language { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

}