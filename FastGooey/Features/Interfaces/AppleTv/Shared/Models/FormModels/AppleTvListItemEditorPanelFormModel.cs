using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.FormModels;

public class AppleTvListItemEditorPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string PosterImage { get; set; } = string.Empty;
    public string LinkToUrl { get; set; } = string.Empty;
}
