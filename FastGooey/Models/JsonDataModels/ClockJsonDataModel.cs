namespace FastGooey.Models.JsonDataModels;

public class ClockJsonDataModel
{
    public string Location { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string Coordinates { get; set; } = string.Empty;
    public string MapIdentifier { get; set; } = string.Empty;
}