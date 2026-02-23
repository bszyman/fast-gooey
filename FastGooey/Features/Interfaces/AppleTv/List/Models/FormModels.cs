using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.List.Models;

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

public class AppleTvListHeaderEditorPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}

public class AppleTvListItemEditorPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string PosterImage { get; set; } = string.Empty;
    public string LinkToUrl { get; set; } = string.Empty;
}

