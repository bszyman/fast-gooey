using FastGooey.Models.ViewModels.NavigationBar;

namespace FastGooey.Features.Widgets.Shell.Models.ViewModels;

public class WidgetsInterfaceSelectorViewModel
{
    public List<InterfaceNavigationItem> InterfaceItems { get; set; } = [];
    public Guid WorkspaceId { get; set; }
}