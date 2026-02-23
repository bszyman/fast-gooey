using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.Alert.Models;

public class AppleTvAlertLowerButtonEditorPanelFormModel
{
    [Required]
    public string LowerButtonText { get; set; } = string.Empty;

    [Required]
    public string LowerButtonLink { get; set; } = string.Empty;
}

public class AppleTvAlertUpperButtonEditorPanelFormModel
{
    [Required]
    public string UpperButtonText { get; set; } = string.Empty;

    [Required]
    public string UpperButtonLink { get; set; } = string.Empty;
}

public class AppleTvAlertWorkspaceFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;
}
