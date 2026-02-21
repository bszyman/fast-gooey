using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.Alert.Models;

public class AppleTvAlertLowerButtonEditorPanelFormModel
{
    [Required]
    public string LowerButtonText { get; set; } = string.Empty;

    [Required]
    public string LowerButtonLink { get; set; } = string.Empty;
}
