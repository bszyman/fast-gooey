using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class Attribution
{
    [JsonPropertyName("logoDark@1x")]
    public string LogoDark1x { get; set; }

    [JsonPropertyName("logoDark@2x")]
    public string LogoDark2x { get; set; }

    [JsonPropertyName("logoDark@3x")]
    public string LogoDark3x { get; set; }

    [JsonPropertyName("logoLight@1x")]
    public string LogoLight1x { get; set; }

    [JsonPropertyName("logoLight@2x")]
    public string LogoLight2x { get; set; }

    [JsonPropertyName("logoLight@3x")]
    public string LogoLight3x { get; set; }

    [JsonPropertyName("logoSquare@1x")]
    public string LogoSquare1x { get; set; }

    [JsonPropertyName("logoSquare@2x")]
    public string LogoSquare2x { get; set; }

    [JsonPropertyName("logoSquare@3x")]
    public string LogoSquare3x { get; set; }

    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
}
