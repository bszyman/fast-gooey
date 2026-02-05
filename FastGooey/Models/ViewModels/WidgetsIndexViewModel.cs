namespace FastGooey.Models.ViewModels;

public class WidgetsIndexViewModel
{
    public Workspace? Workspace { get; set; }
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}