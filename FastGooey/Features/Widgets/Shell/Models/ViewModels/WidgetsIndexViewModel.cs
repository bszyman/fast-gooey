using FastGooey.Models;
using FastGooey.Models.ViewModels;

namespace FastGooey.Features.Widgets.Shell.Models.ViewModels;

public class WidgetsIndexViewModel
{
    public Workspace? Workspace { get; set; }
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}