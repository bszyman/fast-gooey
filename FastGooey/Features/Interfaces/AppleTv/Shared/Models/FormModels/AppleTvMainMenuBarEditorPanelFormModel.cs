namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.FormModels;

public class AppleTvMainMenuBarEditorPanelFormModel
{
    public string Action { get; set; } = "save";
    public int Index { get; set; } = -1;
    public List<AppleTvMainMenuBarButtonFormModel> Items { get; set; } = [];
}

public class AppleTvMainMenuBarButtonFormModel
{
    public string Text { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}
