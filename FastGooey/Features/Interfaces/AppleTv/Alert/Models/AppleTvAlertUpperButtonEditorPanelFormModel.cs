using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.Alert.Models;

public class AppleTvAlertUpperButtonEditorPanelFormModel
{
    [Required]
    public string UpperButtonText { get; set; } = string.Empty;

    [Required]
    public string UpperButtonLink { get; set; } = string.Empty;
}
