namespace FastGooey.Models.ViewModels.NavigationBar;

public class InterfaceNavigationGroup
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<InterfaceNavigationItem> Items { get; set; } = new();
}