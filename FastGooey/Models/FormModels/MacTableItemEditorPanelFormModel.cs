using System.ComponentModel.DataAnnotations;

namespace FastGooey.Models.FormModels;

public class MacTableItemEditorPanelFormModel
{
    [Required]
    public string GooeyName { get; set; } = string.Empty;

    public string? RelatedUrl { get; set; } = string.Empty;

    public string? DoubleClickUrl { get; set; } = string.Empty;
}
