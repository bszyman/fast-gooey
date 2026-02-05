namespace FastGooey.Models.ViewModels;

public class MacInterfaceSelectorViewModel
{
    public List<NavigationBar.InterfaceNavigationItem> InterfaceItems { get; set; } = [];
    public Guid WorkspaceId { get; set; }
}