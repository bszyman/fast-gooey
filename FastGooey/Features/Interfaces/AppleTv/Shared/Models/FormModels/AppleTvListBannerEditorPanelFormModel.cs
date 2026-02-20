using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.FormModels;

public class AppleTvListBannerEditorPanelFormModel
{
    public string? Background { get; set; } = string.Empty;
    public string? Button { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? HeroImg { get; set; } = string.Empty;
    public string? Img { get; set; } = string.Empty;
    public string? Row { get; set; } = string.Empty;
    public string? Stack { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;
}
