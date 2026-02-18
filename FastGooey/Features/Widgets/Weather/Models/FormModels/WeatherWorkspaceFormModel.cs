using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Widgets.Weather.Models.FormModels;

public class WeatherWorkspaceFormModel
{
    [Required]
    public string? Location { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? Coordinates { get; set; }
}
