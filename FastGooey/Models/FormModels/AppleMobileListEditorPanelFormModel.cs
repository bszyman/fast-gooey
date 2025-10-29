using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.FormModels;

public class AppleMobileListEditorPanelFormModel
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}