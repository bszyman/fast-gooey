using System.Text.Json.Serialization;

namespace WeatherKit.Models;

public class DayPartForecast
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
    /// The ending date and time of the forecast.
    /// </summary>
    [JsonPropertyName("forecastEnd")]
    public DateTime ForecastEnd { get; set; }
    
    /// <summary>
    /// The starting date and time of the forecast.
    /// </summary>
    [JsonPropertyName("forecastStart")]
    public DateTime ForecastStart { get; set; }
    
    /// <summary>
    /// The relative humidity during the period, from 0 to 1.
    /// </summary>
    [JsonPropertyName("humidity")]
    public double Humidity { get; set; }
    
    /// <summary>
    /// The amount of precipitation forecasted to occur during the period, in millimeters.
    /// </summary>
    [JsonPropertyName("precipitationAmount")]
    public double PrecipitationAmount { get; set; }
    
    /// <summary>
    /// The chance of precipitation forecasted to occur during the period.
    /// </summary>
    [JsonPropertyName("precipitationChance")]
    public double PrecipitationChance { get; set; }
    
    /// <summary>
    /// The type of precipitation forecasted to occur during the period.
    /// </summary>
    [JsonPropertyName("precipitationType")]
    public PrecipitationType PrecipitationType { get; set; }
    
    /// <summary>
    /// The depth of snow as ice crystals forecasted to occur during the period, in millimeters.
    /// </summary>
    [JsonPropertyName("snowfallAmount")]
    public double SnowfallAmount { get; set; }
    
    /// <summary>
    /// The direction the wind is forecasted to come from during the period, in degrees.
    /// </summary>
    [JsonPropertyName("windDirection")]
    public int? WindDirection { get; set; }
    
    /// <summary>
    /// The average speed the wind is forecasted to be during the period, in kilometers per hour.
    /// </summary>
    [JsonPropertyName("windSpeed")]
    public double WindSpeed { get; set; }

}