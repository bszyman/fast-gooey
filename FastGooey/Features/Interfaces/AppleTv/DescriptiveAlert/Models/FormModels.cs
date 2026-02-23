using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.DescriptiveAlert.Models;

public class DescriptiveAlertDescriptiveContentFormModel
{
    public List<DescriptiveAlertDescriptiveContentNodeFormModel> DescriptiveContent { get; set; } = [];
}

public class DescriptiveAlertDescriptiveContentNodeFormModel
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}


public class DescriptiveAlertWorkspaceFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string CancelButtonText { get; set; } = string.Empty;

    [Required]
    public string ConfirmButtonText { get; set; } = string.Empty;
}