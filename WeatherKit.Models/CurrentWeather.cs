using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class CurrentWeather
{
    /// <summary>
    /// The date and time.
    /// </summary>
    [JsonPropertyName("asOf")]
    public DateTime AsOf { get; set; }

    /// <summary>
    /// The percentage of the sky covered with clouds during the period, from 0 to 1.
    /// </summary>
    [JsonPropertyName("cloudCover")]
    public double? CloudCover { get; set; }

    /// <summary>
    /// An enumeration value indicating the condition at the time.
    /// </summary>
    [JsonPropertyName("conditionCode")]
    public string ConditionCode { get; set; }

    /// <summary>
    /// A Boolean value indicating whether there is daylight.
    /// </summary>
    [JsonPropertyName("daylight")]
    public bool? Daylight { get; set; }

    /// <summary>
    /// The relative humidity, from 0 to 1.
    /// </summary>
    [JsonPropertyName("humidity")]
    public double Humidity { get; set; }

    /// <summary>
    /// The precipitation intensity, in millimeters per hour.
    /// </summary>
    [JsonPropertyName("precipitationIntensity")]
    public double PrecipitationIntensity { get; set; }

    /// <summary>
    /// The sea level air pressure, in millibars.
    /// </summary>
    [JsonPropertyName("pressure")]
    public double Pressure { get; set; }

    /// <summary>
    /// The direction of change of the sea-level air pressure.
    /// </summary>
    [JsonPropertyName("pressureTrend")]
    public PressureTrend PressureTrend { get; set; }

    /// <summary>
    /// The current temperature, in degrees Celsius.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    /// <summary>
    /// The feels-like temperature when factoring wind and humidity, in degrees Celsius.
    /// </summary>
    [JsonPropertyName("temperatureApparent")]
    public double TemperatureApparent { get; set; }

    /// <summary>
    /// The temperature at which relative humidity is 100%, in Celsius.
    /// </summary>
    [JsonPropertyName("temperatureDewPoint")]
    public double TemperatureDewPoint { get; set; }

    /// <summary>
    /// The level of ultraviolet radiation.
    /// </summary>
    [JsonPropertyName("uvIndex")]
    public int UvIndex { get; set; }

    /// <summary>
    /// The distance at which terrain is visible, in meters.
    /// </summary>
    [JsonPropertyName("visibility")]
    public double Visibility { get; set; }

    /// <summary>
    /// The direction of the wind, in degrees.
    /// </summary>
    [JsonPropertyName("windDirection")]
    public int? WindDirection { get; set; }

    /// <summary>
    /// The maximum wind gust speed, in kilometers per hour.
    /// </summary>
    [JsonPropertyName("windGust")]
    public double? WindGust { get; set; }

    /// <summary>
    /// The wind speed, in kilometers per hour.
    /// </summary>
    [JsonPropertyName("windSpeed")]
    public double WindSpeed { get; set; }

}