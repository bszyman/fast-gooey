namespace FastGooey.Models.ViewModels.NavigationBar;

public class NavigationBarViewModel
{
    public Guid WorkspaceId { get; set; }
    public List<WidgetNavigationItem> Widgets { get; set; } = new();
    public List<WidgetNavigationItem> AppleMobileInterfaces { get; set; } = new();
    public List<WidgetNavigationItem> MacOSInterfaces { get; set; } = new();
    public List<WidgetNavigationItem> TvOSInterfaces { get; set; } = new();
}