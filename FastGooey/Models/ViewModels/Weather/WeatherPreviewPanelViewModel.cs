namespace FastGooey.Models.ViewModels.Weather;

public class WeatherPreviewPanelViewModel
{
    public bool PreviewAvailable { get; set; }
    public string Temperature { get; set; } = "-";
    public string Location { get; set; } = string.Empty;
}
