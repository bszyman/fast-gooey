namespace FastGooey.Models.ViewModels;

public class AppleMobileInterfaceSelectorViewModel
{
    public List<NavigationBar.InterfaceNavigationItem> InterfaceItems { get; set; } = [];
    public Guid WorkspaceId { get; set; }
}