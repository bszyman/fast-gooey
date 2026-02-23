using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.Main.Models;

public class AppleTvMainBackgroundEditorPanelFormModel
{
    [Required]
    public string ImageResource { get; set; } = string.Empty;
    public string AudioResource { get; set; } = string.Empty;
}

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
