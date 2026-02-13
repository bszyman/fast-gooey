using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.FormModels;

public class AppleMobileCollectionEditorPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; } = string.Empty;
    public string? Url { get; set; } = string.Empty;
}
