namespace FastGooey.Models.ViewModels.Media;

public class MediaIndexViewModel
{
    public Workspace Workspace { get; set; } = null!;
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}
