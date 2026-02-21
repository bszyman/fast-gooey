using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.DescriptiveAlert.Models;

public class DescriptiveAlertWorkspaceFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string CancelButtonText { get; set; } = string.Empty;

    [Required]
    public string ConfirmButtonText { get; set; } = string.Empty;
}
