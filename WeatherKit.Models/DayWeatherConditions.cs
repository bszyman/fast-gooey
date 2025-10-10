using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class DayWeatherConditions
{
    /// <summary>
    /// An enumeration value indicating the condition at the time.
    /// </summary>
    [JsonPropertyName("conditionCode")]
    public string ConditionCode { get; set; }

    /// <summary>
    /// The forecast between 7 AM and 7 PM for the day.
    /// </summary>
    [JsonPropertyName("daytimeForecast")]
    public DayPartForecast DaytimeForecast { get; set; }

    /// <summary>
    /// The ending date and time of the day.
    /// </summary>
    [JsonPropertyName("forecastEnd")]
    public DateTime ForecastEnd { get; set; }

    /// <summary>
    /// The starting date and time of the day.
    /// </summary>
    [JsonPropertyName("forecastStart")]
    public DateTime ForecastStart { get; set; }

    /// <summary>
    /// The maximum ultraviolet index value during the day.
    /// </summary>
    [JsonPropertyName("maxUvIndex")]
    public int MaxUvIndex { get; set; }

    /// <summary>
    /// The phase of the moon on the specified day.
    /// </summary>
    [JsonPropertyName("moonPhase")]
    public MoonPhase MoonPhase { get; set; }

    /// <summary>
    /// The time of moonrise on the specified day.
    /// </summary>
    [JsonPropertyName("moonrise")]
    public DateTime? Moonrise { get; set; }

    /// <summary>
    /// The time of moonset on the specified day.
    /// </summary>
    [JsonPropertyName("moonset")]
    public DateTime? Moonset { get; set; }

    /// <summary>
    /// The day part forecast between 7 PM and 7 AM for the overnight.
    /// </summary>
    [JsonPropertyName("overnightForecast")]
    public DayPartForecast OvernightForecast { get; set; }

    /// <summary>
    /// The amount of precipitation forecasted to occur during the day, in millimeters.
    /// </summary>
    [JsonPropertyName("precipitationAmount")]
    public double PrecipitationAmount { get; set; }

    /// <summary>
    /// The chance of precipitation forecasted to occur during the day.
    /// </summary>
    [JsonPropertyName("precipitationChance")]
    public double PrecipitationChance { get; set; }

    /// <summary>
    /// The type of precipitation forecasted to occur during the day.
    /// </summary>
    [JsonPropertyName("precipitationType")]
    public PrecipitationType PrecipitationType { get; set; }

    /// <summary>
    /// The depth of snow as ice crystals forecasted to occur during the day, in millimeters.
    /// </summary>
    [JsonPropertyName("snowfallAmount")]
    public double SnowfallAmount { get; set; }

    /// <summary>
    /// The time when the sun is lowest in the sky.
    /// </summary>
    [JsonPropertyName("solarMidnight")]
    public DateTime? SolarMidnight { get; set; }

    /// <summary>
    /// The time when the sun is highest in the sky.
    /// </summary>
    [JsonPropertyName("solarNoon")]
    public DateTime? SolarNoon { get; set; }

    /// <summary>
    /// The time when the top edge of the sun reaches the horizon in the morning.
    /// </summary>
    [JsonPropertyName("sunrise")]
    public DateTime? Sunrise { get; set; }

    /// <summary>
    /// The time when the sun is 18 degrees below the horizon in the morning.
    /// </summary>
    [JsonPropertyName("sunriseAstronomical")]
    public DateTime? SunriseAstronomical { get; set; }

    /// <summary>
    /// The time when the sun is 6 degrees below the horizon in the morning.
    /// </summary>
    [JsonPropertyName("sunriseCivil")]
    public DateTime? SunriseCivil { get; set; }

    /// <summary>
    /// The time when the sun is 12 degrees below the horizon in the morning.
    /// </summary>
    [JsonPropertyName("sunriseNautical")]
    public DateTime? SunriseNautical { get; set; }

    /// <summary>
    /// The time when the top edge of the sun reaches the horizon in the evening.
    /// </summary>
    [JsonPropertyName("sunset")]
    public DateTime? Sunset { get; set; }

    /// <summary>
    /// The time when the sun is 18 degrees below the horizon in the evening.
    /// </summary>
    [JsonPropertyName("sunsetAstronomical")]
    public DateTime? SunsetAstronomical { get; set; }

    /// <summary>
    /// The time when the sun is 6 degrees below the horizon in the evening.
    /// </summary>
    [JsonPropertyName("sunsetCivil")]
    public DateTime? SunsetCivil { get; set; }

    /// <summary>
    /// The time when the sun is 12 degrees below the horizon in the evening.
    /// </summary>
    [JsonPropertyName("sunsetNautical")]
    public DateTime? SunsetNautical { get; set; }

    /// <summary>
    /// The maximum temperature forecasted to occur during the day, in degrees Celsius.
    /// </summary>
    [JsonPropertyName("temperatureMax")]
    public double TemperatureMax { get; set; }

    /// <summary>
    /// The minimum temperature forecasted to occur during the day, in degrees Celsius.
    /// </summary>
    [JsonPropertyName("temperatureMin")]
    public double TemperatureMin { get; set; }

}