namespace FastGooey.Models.JsonDataModels;

public class WeatherJsonDataModel
{
    public string Location { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public string Coordinates { get; set; } = "-° N, -° W";
}