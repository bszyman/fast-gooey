using System.Text.Json.Serialization;

namespace WeatherKit.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Certainty
{
    /// <summary>
    /// The event has already occurred or is ongoing.
    /// </summary>
    [JsonPropertyName("observed")]
    Observed,
    
    /// <summary>
    /// The event is likely to occur (greater than 50% probability).
    /// </summary>
    [JsonPropertyName("likely")]
    Likely,
    
    /// <summary>
    /// The event is unlikley to occur (less than 50% probability).
    /// </summary>
    [JsonPropertyName("possible")]
    Possible,
    
    /// <summary>
    /// The event is not expected to occur (approximately 0% probability).
    /// </summary>
    [JsonPropertyName("unlikely")]
    Unlikely,
    
    /// <summary>
    /// It is unknown if the event will occur.
    /// </summary>
    [JsonPropertyName("unknown")]
    Unknown
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PressureTrend
{
    /// <summary>
    /// The sea level air pressure is increasing.
    /// </summary>
    [JsonPropertyName("rising")]
    Rising,
    
    /// <summary>
    /// The sea level air pressure is decreasing.
    /// </summary>
    [JsonPropertyName("falling")]
    Falling,
    
    /// <summary>
    /// The sea level air pressure is remaining about the same.
    /// </summary>
    [JsonPropertyName("steady")]
    Steady
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PrecipitationType
{
    /// <summary>
    /// No precipitation is occurring.
    /// </summary>
    [JsonPropertyName("clear")]
    Clear,
    
    /// <summary>
    /// An unknown type of precipitation is occuring.
    /// </summary>
    [JsonPropertyName("precipitation")]
    Precipitation,
    
    /// <summary>
    /// Rain or freezing rain is falling. snow Snow is falling.
    /// </summary>
    [JsonPropertyName("rain")]
    Rain,
    
    /// <summary>
    /// The sea level air pressure is remaining about the same.
    /// </summary>
    [JsonPropertyName("snow")]
    Snow,
    
    /// <summary>
    /// Sleet or ice pellets are falling. hail Hail is falling.
    /// </summary>
    [JsonPropertyName("sleet")]
    Sleet,
    
    [JsonPropertyName("hail")]
    Hail,
    
    /// <summary>
    /// Winter weather (wintery mix or wintery showers) is falling.
    /// </summary>
    [JsonPropertyName("mixed")]
    Mixed
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MoonPhase
{
    /// <summary>
    /// The moon isn’t visible.
    /// </summary>
    [JsonPropertyName("new")]
    New,
    
    /// <summary>
    /// A crescent-shaped sliver of the moon is visible, and increasing in size.
    /// </summary>
    [JsonPropertyName("waxingCrescent")]
    WaxingCrescent,
    
    /// <summary>
    /// Approximately half of the moon is visible, and increasing in size.
    /// </summary>
    [JsonPropertyName("firstQuarter")]
    FirstQuarter,
    
    /// <summary>
    /// More than half of the moon is visible, and increasing in size.
    /// </summary>
    [JsonPropertyName("waxingGibbous")]
    WaxingGibbous,
    
    /// <summary>
    /// The entire disc of the moon is visible.
    /// </summary>
    [JsonPropertyName("full")]
    Full,
    
    /// <summary>
    /// More than half of the moon is visible, and decreasing in size.
    /// </summary>
    [JsonPropertyName("waningGibbous")]
    WaningGibbous,
    
    /// <summary>
    /// Approximately half of the moon is visible, and decreasing in size.
    /// </summary>
    [JsonPropertyName("thirdQuarter")]
    ThirdQuarter,
    
    /// <summary>
    /// A crescent-shaped sliver of the moon is visible, and decreasing in size.
    /// </summary>
    [JsonPropertyName("waningCrescent")]
    WaningCrescent
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnitsSystem
{
    /// <summary>
    /// The metric system.
    /// </summary>
    [JsonPropertyName("m")]
    M
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Urgency
{
    /// <summary>
    /// Take responsive action immediately.
    /// </summary>
    [JsonPropertyName("immediate")]
    Immediate,
    
    /// <summary>
    /// Take responsive action in the next hour.
    /// </summary>
    [JsonPropertyName("expected")]
    Expected,
    
    /// <summary>
    /// TTake responsive action in the near future.
    /// </summary>
    [JsonPropertyName("future")]
    Future,
    
    /// <summary>
    /// Responsive action is no longer required.
    /// </summary>
    [JsonPropertyName("past")]
    Past,
    
    /// <summary>
    /// The urgency is unknown.
    /// </summary>
    [JsonPropertyName("unknown")]
    Unknown
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Severity
{
    /// <summary>
    /// Extraordinary threat.
    /// </summary>
    [JsonPropertyName("extreme")]
    Extreme,
    
    /// <summary>
    /// Significant threat.
    /// </summary>
    [JsonPropertyName("severe")]
    Severe,
    
    /// <summary>
    /// Possible threat.
    /// </summary>
    [JsonPropertyName("moderate")]
    Moderate,
    
    /// <summary>
    /// Minimal or no known threat.
    /// </summary>
    [JsonPropertyName("minor")]
    Minor,
    
    /// <summary>
    /// Unknown threat.
    /// </summary>
    [JsonPropertyName("unknown")]
    Unknown
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResponseType
{
    [JsonPropertyName("shelter")]
    Shelter,
    
    [JsonPropertyName("evacuate")]
    Evacuate,
    
    [JsonPropertyName("prepare")]
    Prepare,
    
    [JsonPropertyName("execute")]
    Execute,
    
    [JsonPropertyName("avoid")]
    Avoid,
    
    [JsonPropertyName("monitor")]
    Monitor,
    
    [JsonPropertyName("assess")]
    Assess,
    
    [JsonPropertyName("allClear")]
    AllClear,
    
    [JsonPropertyName("none")]
    None
}

/// <summary>
/// Defines the types of weather data that can be requested or included in a response.
/// </summary>
public enum DataSet
{
    /// <summary>
    /// Current weather conditions
    /// </summary>
    CurrentWeather,
    
    /// <summary>
    /// Daily forecast information
    /// </summary>
    ForecastDaily,
    
    /// <summary>
    /// Hourly forecast information
    /// </summary>
    ForecastHourly,
    
    /// <summary>
    /// Next hour detailed forecast
    /// </summary>
    ForecastNextHour,
    
    /// <summary>
    /// Weather alerts and warnings
    /// </summary>
    WeatherAlerts
}
