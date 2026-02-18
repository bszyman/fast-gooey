namespace FastGooey.Features.Widgets.Map.Models.JsonDataModels;

public class MapWorkspacePinModel
{
    public Guid EntryId { get; set; }
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public string Coordinates { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string LocationIdentifier { get; set; } = string.Empty;
}

public class MapJsonDataModel
{
    public List<MapWorkspacePinModel> Pins { get; set; } = [];
}