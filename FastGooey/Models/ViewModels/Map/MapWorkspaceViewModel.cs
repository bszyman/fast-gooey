namespace FastGooey.Models.ViewModels.Map;

public class MapWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public IEnumerable<MapCityEntryViewModel> Entries { get; set; } = [];
}