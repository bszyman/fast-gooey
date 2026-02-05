namespace FastGooey.Models.ViewModels;

public class MacOSIndexViewModel
{
    public Workspace? Workspace { get; set; }
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}