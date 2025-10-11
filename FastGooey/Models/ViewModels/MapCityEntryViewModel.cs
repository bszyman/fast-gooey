namespace FastGooey.Models.ViewModels;

public class MapCityEntryViewModel
{
    public int Index { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationIdentifier { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string CoordinateDisplay { get; set; } = string.Empty;
}