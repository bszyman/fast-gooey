namespace FastGooey.Models.ViewModels;

public class MacOSIndexViewModel
{
    public Guid WorkspaceId { get; set; }
    public MetalNavBarViewModel NavBarViewModel { get; set; } = new();
}