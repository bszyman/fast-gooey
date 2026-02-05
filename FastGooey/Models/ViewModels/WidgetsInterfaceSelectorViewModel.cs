namespace FastGooey.Models.ViewModels;

public class WidgetsInterfaceSelectorViewModel
{
    public List<NavigationBar.InterfaceNavigationItem> InterfaceItems { get; set; } = [];
    public Guid WorkspaceId { get; set; }
}