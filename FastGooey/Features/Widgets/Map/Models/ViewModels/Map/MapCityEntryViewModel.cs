namespace FastGooey.Features.Widgets.Map.Models.ViewModels.Map;

public class MapCityEntryViewModel
{
    public Guid EntryId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationIdentifier { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string CoordinateDisplay { get; set; } = string.Empty;
}