using FastGooey.Utils;
namespace FastGooey.Features.Widgets.Weather.Models.ViewModels.Weather;

public class WeatherViewModel
{
    public WeatherWorkspaceViewModel? WorkspaceViewModel { get; set; }

    public string WorkspaceId()
    {
        return WorkspaceViewModel!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return WorkspaceViewModel!.ContentNode!.DocId.ToBase64Url();
    }
}