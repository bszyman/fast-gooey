using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.Mac.Shared.Models.FormModels;

public class MacSourceListGroupItemPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Icon { get; set; } = string.Empty;

    public string? Url { get; set; } = string.Empty;
}
