namespace FastGooey.Models.ViewModels.NavigationBar;

public class NavigationBarViewModel
{
    public Guid WorkspaceId { get; set; }
    public List<WidgetNavigationItem> Widgets { get; set; } = new();
    public List<InterfaceNavigationItem> AppleMobileInterfaces { get; set; } = new();
    public List<InterfaceNavigationItem> MacOSInterfaces { get; set; } = new();
    public List<InterfaceNavigationItem> TvOSInterfaces { get; set; } = new();
}