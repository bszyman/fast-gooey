using FastGooey.Models;
using FastGooey.Models.ViewModels;

namespace FastGooey.Features.Media.Shared.Models.ViewModels.Media;

public class MediaIndexViewModel
{
    public Workspace Workspace { get; set; } = null!;
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}
