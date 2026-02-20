using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.FormModels;

public class AppleTvListHeaderEditorPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}
