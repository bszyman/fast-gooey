
namespace FastGooey.Models.ViewModels.AppleTv;

public class AppleTvIndexViewModel
{
    public Workspace? Workspace { get; set; }
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}