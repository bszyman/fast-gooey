
using FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv.Accessories;

namespace FastGooey.Features.Interfaces.AppleTv.Main.Models;

public class MainJsonDataModel
{
    public BackgroundSplash BackgroundSplash { get; set; } = new ();
    public List<NavigationButtonJsonDataModel> MenuBarButtons { get; set; } = [];
}