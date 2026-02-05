namespace FastGooey.Models.ViewModels;

public class AppleMobileIndexViewModel
{
    public Workspace? Workspace { get; set; }
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}