namespace FastGooey.Models.Configuration;

public class WeatherKitConfigurationModel
{
    public bool Enabled { get; init; }
    public string? Origin { get; init; }
    public string? KeyId { get; init; }
    public string? TeamId { get; init; }
    public string? KeyLocation { get; init; }
    public string? DefaultTimezone { get; init; }
}