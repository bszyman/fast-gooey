namespace MapKit.Models;

public class MapKitCoordinateModel
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string CoordinateDisplay()
    {
        return (Longitude < 0) ?
            $"{Latitude}° N, {Math.Abs(Longitude.Value)}° W" :
            $"{Latitude}° N, {Longitude}° E";
    }
}