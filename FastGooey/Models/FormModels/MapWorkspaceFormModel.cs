namespace FastGooey.Models.FormModels;

public class MapWorkspaceFormModel
{
    public List<MapLocationEntryFormModel> Locations { get; set; } = new();
}

public class MapLocationEntryFormModel
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Coordinates { get; set; } = string.Empty;
    public Guid EntryId { get; set; } = Guid.Empty;
    public string LocationName { get; set; } = string.Empty;
}