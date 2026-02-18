using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.FormModels;

public class AppleTvMainBackgroundEditorPanelFormModel
{
    [Required]
    public string ImageResource { get; set; } = string.Empty;
    public string AudioResource { get; set; } = string.Empty;
}
