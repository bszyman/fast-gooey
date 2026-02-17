using FastGooey.Models.JsonDataModels.AppleTv.Accessories;

namespace FastGooey.Models.JsonDataModels.AppleTv;

public class MainJsonDataModel
{
    public BackgroundSplash BackgroundSplash { get; set; } = new ();
    public List<NavigationButtonJsonDataModel> MenuBarButtons { get; set; } = [];
}