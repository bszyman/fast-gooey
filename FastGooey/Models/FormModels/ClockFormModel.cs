using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.FormModels;

public class ClockFormModel
{
    public string? Location { get; set; }
    public string? Timezone { get; set; }
    public string? Coordinates { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    [Required(ErrorMessage = "Please select a location from the search results")]
    public string? MapIdentifier { get; set; }
}
