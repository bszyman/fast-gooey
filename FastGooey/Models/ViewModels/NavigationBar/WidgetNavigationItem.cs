namespace FastGooey.Models.ViewModels.NavigationBar;

public class WidgetNavigationItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
}