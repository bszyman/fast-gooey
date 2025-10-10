namespace MapKit.Models;

public class MapKitSearchResultsResponseModel
{
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public string? Name { get; set; }
    public string? Id { get; set; }
    public MapKitCoordinateModel? Coordinate { get; set; }
    public MapKitStructuredAddressResponseModel? StructuredAddress { get; set; }
}