using FastGooey.Models;
using FastGooey.Utils;
namespace FastGooey.Features.Widgets.Map.Models.ViewModels.Map;

public class MapWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public IEnumerable<MapCityEntryViewModel> Entries { get; set; } = [];

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}