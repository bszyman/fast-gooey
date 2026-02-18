using FastGooey.Models;
using FastGooey.Models.ViewModels;

namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.ViewModels.AppleTv;

public class AppleTvIndexViewModel
{
    public Workspace? Workspace { get; set; }
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}