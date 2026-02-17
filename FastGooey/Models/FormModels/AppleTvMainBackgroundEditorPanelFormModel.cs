using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.FormModels;

public class AppleTvMainBackgroundEditorPanelFormModel
{
    [Required]
    public string ImageResource { get; set; } = string.Empty;
    public string AudioResource { get; set; } = string.Empty;
}
