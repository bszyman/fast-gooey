using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.FormModels;

public class RelatedItemPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Link { get; set; } = string.Empty;

    [Required]
    public string MediaUrl { get; set; } = string.Empty;
}
