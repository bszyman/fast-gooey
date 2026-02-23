using System.ComponentModel.DataAnnotations;

namespace FastGooey.Features.Interfaces.AppleMobile.Collection.Models;

public class AppleMobileCollectionEditorPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; } = string.Empty;
    public string? Url { get; set; } = string.Empty;
}