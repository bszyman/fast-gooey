using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class HourWeatherConditions
{
    /// <summary>
    /// The percentage of the sky covered with clouds during the period, from 0 to 1.
    /// </summary>
    [JsonPropertyName("cloudCover")]
    public double CloudCover { get; set; }

    /// <summary>
    /// An enumeration value indicating the condition at the time.
    /// </summary>
    [JsonPropertyName("conditionCode")]
    public string ConditionCode { get; set; }

    /// <summary>
    /// Indicates whether the hour starts during the day or night.
    /// </summary>
    [JsonPropertyName("daylight")]
    public bool? Daylight { get; set; }

    /// <summary>
    /// The starting date and time of the forecast.
    /// </summary>
    [JsonPropertyName("forecastStart")]
    public DateTime ForecastStart { get; set; }

    /// <summary>
    /// The relative humidity at the start of the hour, from 0 to 1.
    /// </summary>
    [JsonPropertyName("humidity")]
    public double Humidity { get; set; }

    /// <summary>
    /// The chance of precipitation forecasted to occur during the hour, from 0 to 1.
    /// </summary>
    [JsonPropertyName("precipitationChance")]
    public double PrecipitationChance { get; set; }

    /// <summary>
    /// The type of precipitation forecasted to occur during the period.
    /// </summary>
    [JsonPropertyName("precipitationType")]
    public PrecipitationType PrecipitationType { get; set; }

    /// <summary>
    /// The sea-level air pressure, in millibars.
    /// </summary>
    [JsonPropertyName("pressure")]
    public double Pressure { get; set; }

    /// <summary>
    /// The direction of change of the sea-level air pressure.
    /// </summary>
    [JsonPropertyName("pressureTrend")]
    public PressureTrend? PressureTrend { get; set; }

    /// <summary>
    /// The rate at which snow crystals are falling, in millimeters per hour.
    /// </summary>
    [JsonPropertyName("snowfallIntensity")]
    public double? SnowfallIntensity { get; set; }

    /// <summary>
    /// The temperature at the start of the hour, in degrees Celsius.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    /// <summary>
    /// The feels-like temperature when considering wind and humidity, at the start of the hour, in degrees Celsius.
    /// </summary>
    [JsonPropertyName("temperatureApparent")]
    public double TemperatureApparent { get; set; }

    /// <summary>
    /// The temperature at which relative humidity is 100% at the top of the hour, in degrees Celsius.
    /// </summary>
    [JsonPropertyName("temperatureDewPoint")]
    public double? TemperatureDewPoint { get; set; }

    /// <summary>
    /// The level of ultraviolet radiation at the start of the hour.
    /// </summary>
    [JsonPropertyName("uvIndex")]
    public int UvIndex { get; set; }

    /// <summary>
    /// The distance at which terrain is visible at the start of the hour, in meters.
    /// </summary>
    [JsonPropertyName("visibility")]
    public double Visibility { get; set; }

    /// <summary>
    /// The direction of the wind at the start of the hour, in degrees.
    /// </summary>
    [JsonPropertyName("windDirection")]
    public int? WindDirection { get; set; }

    /// <summary>
    /// The maximum wind gust speed during the hour, in kilometers per hour.
    /// </summary>
    [JsonPropertyName("windGust")]
    public double? WindGust { get; set; }

    /// <summary>
    /// The wind speed at the start of the hour, in kilometers per hour.
    /// </summary>
    [JsonPropertyName("windSpeed")]
    public double WindSpeed { get; set; }

    /// <summary>
    /// The amount of precipitation forecasted to occur during period, in millimeters.
    /// </summary>
    [JsonPropertyName("precipitationAmount")]
    public double? PrecipitationAmount { get; set; }

}